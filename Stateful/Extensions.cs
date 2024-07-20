using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Stateful;

/// <summary>
/// Various helper extensions
/// </summary>
public static class Extensions {
    /// <summary>
    /// Awaits an object if it's a task
    /// </summary>
    /// <param name="obj">Object</param>
    public static async Task AwaitIfTask(this object? obj) {
        if (obj is Task task) await task;
    }

    /// <summary>
    /// Get Chat ID from update
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>Chat ID</returns>
    internal static long? GetChatId(this Update update)
        => update.Type switch {
            UpdateType.CallbackQuery => update.CallbackQuery!.Message!.Chat.Id,
            UpdateType.EditedMessage => update.EditedMessage!.Chat.Id,
            UpdateType.ChannelPost => update.ChannelPost!.Chat.Id,
            UpdateType.ChatMember => update.ChannelPost!.Chat.Id,
            UpdateType.Message => update.Message!.Chat.Id,
            _ => null
        };
    
    /// <summary>
    /// Get User ID from update
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>User ID</returns>
    internal static long? GetUserId(this Update update)
        => update.Type switch {
            UpdateType.EditedChannelPost => update.EditedChannelPost!.From!.Id,
            UpdateType.PreCheckoutQuery => update.PreCheckoutQuery!.From.Id,
            UpdateType.ChatJoinRequest => update.ChatJoinRequest!.From.Id,
            UpdateType.EditedMessage => update.EditedMessage!.From!.Id,
            UpdateType.CallbackQuery => update.CallbackQuery!.From.Id,
            UpdateType.ShippingQuery => update.ShippingQuery!.From.Id,
            UpdateType.ChannelPost => update.ChannelPost!.From!.Id,
            UpdateType.ChatMember => update.ChannelPost!.From!.Id,
            UpdateType.InlineQuery => update.InlineQuery!.From.Id,
            UpdateType.PollAnswer => update.PollAnswer!.User!.Id,
            UpdateType.Message => update.Message!.From!.Id,
            _ => null
        };
    
    /// <summary>
    /// Get Message ID from update
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>Message ID</returns>
    internal static int? GetMessageId(this Update update)
        => update.Type switch {
            UpdateType.CallbackQuery => update.CallbackQuery!.Message!.MessageId,
            UpdateType.EditedChannelPost => update.EditedChannelPost!.MessageId,
            UpdateType.EditedMessage => update.EditedMessage!.MessageId,
            UpdateType.ChannelPost => update.ChannelPost!.MessageId,
            UpdateType.ChatMember => update.ChannelPost!.MessageId,
            UpdateType.Message => update.Message!.MessageId,
            _ => null
        };

    /// <summary>
    /// Put state data
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="handler">Update Handler</param>
    public static async Task<Message> PutState(this Task<Message> message, UpdateHandler handler) {
        var msg = await message;
        var state = new MessageState {
            MessageId = msg.MessageId, ChatId = msg.Chat.Id,
            HandlerId = handler.State.HandlerId,
            State = handler.State.State,
            LastUpdated = DateTime.Now
        };

        await MongoState.States.InsertOneAsync(state);
        handler.State = state;
        return msg;
    }
}