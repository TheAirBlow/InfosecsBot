using Stateful.Attributes;
using Telegram.Bot.Types.Enums;

namespace Stateful.AutoKeyboard;

/// <summary>
/// Internal stateful handler attribute that checks for callback data
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal class InternalCallbackAttribute : HandlerAttribute {
    /// <summary>
    /// Does callback data equal to
    /// </summary>
    /// <param name="data">Data</param>
    public InternalCallbackAttribute(string data)
        => Condition = handler => Task.FromResult(handler.Update.CallbackQuery?.Data?.StartsWith($"stinternal-{data}") ?? false);
}