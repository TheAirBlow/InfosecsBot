using Stateful.Attributes;
using Stateful.Keyboards;

namespace Stateful.AutoKeyboard;

/// <summary>
/// Keyboard generator extensions
/// </summary>
public static class KeyboardGenerator {
    /// <summary>
    /// Generates a reply keyboard for this handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>Generated reply keyboard</returns>
    public static ReplyKeyboard GenerateReply(this UpdateHandler handler) {
        var list = new List<string>();
        foreach (var method in handler.GetType().GetMethods(StatefulBot.Flags)) {
            var attrs = method.GetCustomAttributes(false);
            var msg = (MessageAttribute?)attrs.FirstOrDefault(x => x is MessageAttribute);
            if (msg == null || msg.Hidden || msg.Message == null) continue;
            if (attrs.Where(x => x is not MessageAttribute && x is HandlerAttribute)
                .Any(x => x is HandlerAttribute attr && !attr.Match(handler))) continue;
            list.Add(msg.Message);
        }
        return new ReplyKeyboard(list.ToArray());
    }
    
    /// <summary>
    /// Generates a reply keyboard for this handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>Generated reply keyboard</returns>
    public static InlineKeyboard GenerateInline(this UpdateHandler handler) {
        var dict = new Dictionary<string, string>();
        foreach (var method in handler.GetType().GetMethods(StatefulBot.Flags)) {
            var attrs = method.GetCustomAttributes(false);
            var call = (CallbackAttribute?)attrs.FirstOrDefault(x => x is CallbackAttribute);
            if (call == null || call.Hidden || call.Data == null) continue;
            if (attrs.Where(x => x is not CallbackAttribute && x is HandlerAttribute)
                .Any(x => x is HandlerAttribute attr && !attr.Match(handler))) continue;
            dict.Add(call.Name ?? call.Data, call.Data.TrimEnd('\n'));
        }
        return new InlineKeyboard(dict);
    }
}