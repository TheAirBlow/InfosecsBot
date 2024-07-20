using Telegram.Bot.Types.ReplyMarkups;

namespace Stateful.Keyboards;

/// <summary>
/// A simpler inline keyboard
/// </summary>
public class InlineKeyboard : InlineKeyboardMarkup {
    /// <summary>
    /// Creates a new inline keyboard from it's native counterpart
    /// </summary>
    /// <param name="markup">Inline keyboard markup</param>
    public InlineKeyboard(InlineKeyboardMarkup markup)
        : base(markup.InlineKeyboard) { }

    /// <summary>
    /// Creates a new inline keyboard from a list of button names.
    /// To make the next button appear from a new line, add a newline at the end.
    /// </summary>
    /// <param name="buttons">List of buttons</param>
    public InlineKeyboard(params string[] buttons) 
        : base(GenerateMarkupList(buttons)) { }
    
    /// <summary>
    /// Creates a new inline keyboard from a dictionary with keys and values being display names and callback data respectively.
    /// To make the next button appear from a new line, add a newline at the end of a display name.
    /// </summary>
    /// <param name="buttons">Button dictionary</param>
    public InlineKeyboard(Dictionary<string, string> buttons) 
        : base(GenerateMarkupList(buttons)) { }

    /// <summary>
    /// Generates a markup list
    /// </summary>
    /// <param name="buttons">Button dictionary</param>
    /// <returns>Markup list</returns>
    private static List<List<InlineKeyboardButton>> GenerateMarkupList(Dictionary<string, string> buttons) {
        var list = new List<List<InlineKeyboardButton>>();
        var current = new List<InlineKeyboardButton>();
        list.Add(current);
        foreach (var button in buttons) {
            current.Add(new InlineKeyboardButton(button.Key.TrimEnd('\n')) 
                { CallbackData = button.Value });

            if (!button.Key.EndsWith('\n')) continue;
            current = []; list.Add(current);
        }

        return list;
    }
    
    /// <summary>
    /// Generates a markup list
    /// </summary>
    /// <param name="buttons">Button names</param>
    /// <returns>Markup list</returns>
    private static List<List<InlineKeyboardButton>> GenerateMarkupList(string[] buttons) {
        var list = new List<List<InlineKeyboardButton>>();
        var current = new List<InlineKeyboardButton>();
        list.Add(current);
        foreach (var button in buttons) {
            current.Add(new InlineKeyboardButton(button.TrimEnd('\n')) 
                { CallbackData = button.TrimEnd('\n') });

            if (!button.EndsWith('\n')) continue;
            current = []; list.Add(current);
        }

        return list;
    }
}