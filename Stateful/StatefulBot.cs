using System.Reflection;
using Serilog;
using Stateful.Attributes;
using Stateful.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Stateful;

/// <summary>
/// Stateful logic implementation
/// </summary>
public class StatefulBot {
    /// <summary>
    /// Binding flags to use for searching methods
    /// </summary>
    internal const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
    
    /// <summary>
    /// List of update handlers
    /// </summary>
    private readonly Dictionary<string, Type> _handlers = new();

    /// <summary>
    /// Telegram bot client instance
    /// </summary>
    private readonly TelegramBotClient _client;

    /// <summary>
    /// Creates a new stateful bot
    /// </summary>
    /// <param name="client">Bot Client</param>
    public StatefulBot(TelegramBotClient client)
        => _client = client;
    
    /// <summary>
    /// Registers a handler
    /// </summary>
    /// <param name="id">Unique ID</param>
    /// <typeparam name="T">Type</typeparam>
    public void Register<T>(string id) where T : UpdateHandler
        => _handlers.Add(id, typeof(T));
    
    /// <summary>
    /// Handles a polled update
    /// </summary>
    /// <param name="update">Update</param>
    public async Task HandleUpdate(Update update) {
        try {
            switch (update.Type) {
                case UpdateType.Message:
                    //if (update.Message!.Chat.Type != ChatType.Private) return;
                    if (update.Message!.Chat.Type != ChatType.Group) return;
                    break;
                case UpdateType.EditedMessage:
                    //if (update.EditedMessage!.Chat.Type != ChatType.Private) return;
                    if (update.EditedMessage!.Chat.Type != ChatType.Group) return;
                    break;
                case UpdateType.CallbackQuery:
                    await _client.AnswerCallbackQueryAsync(update.CallbackQuery!.Id);
                    break;
                default:
                    return;
            }
            var state = await MessageState.Get(update);
            if (state.HandlerId == null || !_handlers.TryGetValue(state.HandlerId, out var type))
                type = _handlers.Values.First();
            
            // Hardcoded internal handler, please forgive me
            var handler = CreateHandler(typeof(InternalHandler), state, update);
            var method = GetMethod(handler);
            if (method == null) {
                handler = CreateHandler(type, state, update);
                method = GetMethod(handler);
                if (method == null) {
                    Log.Warning("Failed to find a handler in {0}. User: {1}, Message: {2}", 
                        type.Name, update.Message?.From?.Id, update.Message?.MessageId);
                    return;
                }
            }
            
            await Invoke(method, handler);
        } catch (Exception e) {
            Log.Fatal("Handling update failed: {0}", e);
        }
    }

    /// <summary>
    /// Invokes update handler's method
    /// </summary>
    /// <param name="method">Method</param>
    /// <param name="handler">Update Handler</param>
    private async Task Invoke(MethodBase method, UpdateHandler handler) {
        try {
            await method.Invoke(handler, []).AwaitIfTask();
        } catch (Exception e) {
            Log.Error("Invocation of {0}.{1} failed: {2}",
                method.DeclaringType!.Name, method.Name, e);
        }
    }
    
    /// <summary>
    /// Creates an update handler
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="state">State</param>
    /// <param name="update">Update</param>
    /// <returns></returns>
    private UpdateHandler CreateHandler(Type type, MessageState state, Update update) {
        var handler = (UpdateHandler)Activator.CreateInstance(type)!;
        handler.Client = _client; handler.Stateful = this;
        handler.State = state; handler.Update = update;
        return handler;
    }

    /// <summary>
    /// Returns handler method to call
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>Handler Method</returns>
    private MethodBase? GetMethod(UpdateHandler handler) {
        var method = handler.GetType().GetMethods(Flags).FirstOrDefault(x => {
            var attrs = x.GetCustomAttributes(false);
            if (attrs.Any(j => j is DefaultHandlerAttribute)) return false;
            var handlers = attrs.Where(j => j is HandlerAttribute);
            return handlers.Any() && handlers.All(j => j is HandlerAttribute attr && attr.Match(handler));
        });

        if (handler.Update.Type != UpdateType.CallbackQuery)
            method ??= GetDefault(handler);
        return method;
    }
        
    /// <summary>
    /// Returns default method in a handler class
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>Default Handler</returns>
    private static MethodInfo? GetDefault(UpdateHandler handler)
        => handler.GetType().GetMethods(Flags).FirstOrDefault(x => {
            var attrs = x.GetCustomAttributes(false);
            return attrs.Any(j => j is DefaultHandlerAttribute) && attrs.Where(j => j is HandlerAttribute)
                .All(j => j is HandlerAttribute attr && attr.Match(handler));
        });

    /// <summary>
    /// Runs default method of a handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="id">Handler ID</param>
    internal async Task RunDefault(UpdateHandler handler, string id) {
        if (!_handlers.TryGetValue(id, out var type))
            throw new ArgumentOutOfRangeException(nameof(id),
                $"No handler with ID {id} was registered");
        handler = CreateHandler(type, handler.State, handler.Update);
        var method = GetDefault(handler);
        if (method != null) await Invoke(method, handler);
    }
}