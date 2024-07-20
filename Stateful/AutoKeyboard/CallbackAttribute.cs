using Stateful.Attributes;
using Telegram.Bot.Types.Enums;

namespace Stateful.AutoKeyboard;

/// <summary>
/// Stateful handler attribute that checks for callback data
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CallbackAttribute : HandlerAttribute {
    /// <summary>
    /// Name of the button
    /// </summary>
    public string? Name { get; }
    
    /// <summary>
    /// Callback data
    /// </summary>
    public string? Data { get; }
    
    /// <summary>
    /// Hidden from auto generator
    /// </summary>
    public bool Hidden { get; }

    /// <summary>
    /// Does callback data equal to
    /// </summary>
    /// <param name="data">Data</param>
    /// <param name="name">Name</param>
    /// <param name="hidden">Hidden</param>
    public CallbackAttribute(string? data = null, string? name = null, bool hidden = false) {
        Condition = handler => handler.Update.Type != UpdateType.CallbackQuery ? Task.FromResult(false)
            : Task.FromResult(data == null || handler.Update.CallbackQuery!.Data == data.TrimEnd('\n'));
        Name = name; Data = data; Hidden = hidden;
    }
}