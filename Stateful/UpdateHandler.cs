using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Stateful; 

/// <summary>
/// Abstract update handler class. Keep in mind that
/// a single instance of your update handler will be used
/// to handle updates, so don't assume non-static fields
/// or properties are per-update or per-user.
/// </summary>
public abstract class UpdateHandler {
    /// <summary>
    /// Telegram bot client instance
    /// </summary>
    public TelegramBotClient Client { get; internal set; }
    
    /// <summary>
    /// Stateful bot instance
    /// </summary>
    public StatefulBot Stateful { get; internal set; }
    
    /// <summary>
    /// Current message state
    /// </summary>
    public MessageState State { get; internal set; }
    
    /// <summary>
    /// Handled update instance
    /// </summary>
    public Update Update { get; internal set; }

    /// <summary>
    /// Telegram Chat ID
    /// </summary>
    public long? ChatId => Update.GetChatId();
    
    /// <summary>
    /// Telegram User ID
    /// </summary>
    public long? UserId => Update.GetUserId();
    
    /// <summary>
    /// Telegram Message ID
    /// </summary>
    public int? MessageId => Update.GetMessageId();

    /// <summary>
    /// Changes handler and runs it's default method
    /// </summary>
    /// <param name="id">Handler ID</param>
    /// <param name="runDefault">Run default</param>
    protected async Task ChangeHandler(string id, bool runDefault = true) {
        await State.SetHandler(id);
        if (runDefault) await Stateful.RunDefault(this, id);
    }

    /// <summary>
    /// Use this method to send text messages.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel</param>
    /// <param name="text">Text of the message to be sent, 1-4096 characters after entities parsing</param>
    /// <param name="messageThreadId">Unique identifier of forum's topic</param>
    /// <param name="parseMode">Mode for parsing entities in the new caption</param>
    /// <param name="entities">List of special entities that appear in message text</param>
    /// <param name="disableNotification">Sends the message silently. Users will receive a notification with no sound</param>
    /// <param name="protectContent">Protects the contents of sent messages from forwarding and saving</param>
    /// <param name="replyToMessageId">If the message is a reply, ID of the original message</param>
    /// <param name="allowSendingWithoutReply">Should the message should be sent even if the specified replied-to message is not found</param>
    /// <param name="keyboard">Additional interface options</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>On success, the sent <see cref="Message"/> is returned.</returns>
    protected async Task<Message> SendMessage(string text, IReplyMarkup? keyboard = null, 
        ChatId? chatId = null, ParseMode? parseMode = ParseMode.Markdown, int? messageThreadId = null,
        IEnumerable<MessageEntity>? entities = null, bool? disableNotification = null, bool? protectContent = null,
        int? replyToMessageId = null, bool? allowSendingWithoutReply = null, CancellationToken cancellationToken = default) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(
            nameof(chatId), "both chatId and ChatId are null");
        var msg = await Client.SendTextMessageAsync(chatId, text, messageThreadId, parseMode, entities,
            disableNotification, disableNotification, protectContent, replyToMessageId,
            allowSendingWithoutReply, keyboard, cancellationToken).PutState(this);
        await State.ApplyChanges();
        return msg;
    }

    /// <summary>
    /// Use this method to edit text and game messages.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel</param>
    /// <param name="messageId">Identifier of the message to edit</param>
    /// <param name="text">New text of the message, 1-4096 characters after entities parsing</param>
    /// <param name="parseMode">Mode for parsing entities in the new caption</param>
    /// <param name="entities">List of special entities that appear in message text</param>
    /// <param name="disableWebPagePreview">Disables link previews for links in this message</param>
    /// <param name="keyboard">Additional interface options</param>
    /// <param name="cancellationToken"> A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>On success the edited <see cref="Message"/> is returned.</returns>
    protected async Task<Message> EditMessage(string text, InlineKeyboardMarkup? keyboard = default,
        ChatId? chatId = null, int? messageId = null, ParseMode? parseMode = ParseMode.Markdown,
        IEnumerable<MessageEntity>? entities = default, bool? disableWebPagePreview = default,
        CancellationToken cancellationToken = default) {
        chatId ??= ChatId; messageId ??= MessageId;
        if (chatId == null) throw new ArgumentNullException(
            nameof(chatId), "both chatId and ChatId are null");
        if (messageId == null) throw new ArgumentNullException(
            nameof(messageId), "both messageId and MessageId are null");
        try {
            var msg = await Client.EditMessageTextAsync(chatId, messageId.Value, text, parseMode,
                entities, disableWebPagePreview, keyboard, cancellationToken);
            await State.ApplyChanges();
            return msg;
        } catch (ApiRequestException e) {
            if (e.Message.Contains("message is not modified"))
                return null!; // let's hope I don't use the returned message!
            throw;
        }
    }
    
    /// <summary>
    /// Edits current message if the update was a callback query, otherwise sends a new one.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel</param>
    /// <param name="messageId">Identifier of the message to edit</param>
    /// <param name="text">New text of the message, 1-4096 characters after entities parsing</param>
    /// <param name="parseMode">Mode for parsing entities in the new caption</param>
    /// <param name="entities">List of special entities that appear in message text</param>
    /// <param name="disableWebPagePreview">Disables link previews for links in this message</param>
    /// <param name="keyboard">Additional interface options</param>
    /// <param name="cancellationToken"> A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>On success the edited <see cref="Message"/> is returned.</returns>
    protected async Task<Message> SendOrEditMessage(string text, InlineKeyboardMarkup? keyboard = default,
        ChatId? chatId = null, ParseMode? parseMode = ParseMode.Markdown, IEnumerable<MessageEntity>? entities = default,
        bool? disableWebPagePreview = default, CancellationToken cancellationToken = default) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(
            nameof(chatId), "both chatId and ChatId are null");
        if (Update.Type == UpdateType.CallbackQuery)
            return await EditMessage(text, keyboard, chatId, MessageId, parseMode,
                entities, disableWebPagePreview, cancellationToken);
        return await SendMessage(text, keyboard, chatId, parseMode,
            entities: entities, cancellationToken: cancellationToken);
    }
}