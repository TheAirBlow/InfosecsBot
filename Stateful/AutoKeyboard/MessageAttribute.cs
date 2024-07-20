using Stateful.Attributes;
using Telegram.Bot.Types.Enums;

namespace Stateful.AutoKeyboard;

/// <summary>
/// Stateful handler attribute that checks for message text
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class MessageAttribute : HandlerAttribute {
    /// <summary>
    /// Message text
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Hidden from auto generator
    /// </summary>
    public bool Hidden { get; }

    /// <summary>
    /// Does update message text equal to
    /// </summary>
    /// <param name="msg">Message</param>
    /// <param name="hidden">Hidden</param>
    public MessageAttribute(string? msg = null, bool hidden = false) {
        Condition = handler => {
            if (handler.Update.Type != UpdateType.Message || handler.Update.Message!.Type != MessageType.Text)
                return Task.FromResult(false);
            return Task.FromResult(msg == null || handler.Update.Message!.Text == msg.TrimEnd('\n'));
        };
        Message = msg; Hidden = hidden;
    }
}