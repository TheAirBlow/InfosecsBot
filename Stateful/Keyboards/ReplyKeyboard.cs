using Telegram.Bot.Types.ReplyMarkups;

namespace Stateful.Keyboards;

/// <summary>
/// A simpler reply keyboard
/// </summary>
public class ReplyKeyboard : ReplyKeyboardMarkup {
    /// <summary>
    /// Creates a new reply keyboard from it's native counterpart
    /// </summary>
    /// <param name="markup">Reply keyboard markup</param>
    public ReplyKeyboard(ReplyKeyboardMarkup markup)
        : base(markup.Keyboard) { }

    /// <summary>
    /// Creates a new reply keyboard from a list of button names.
    /// To make the next button appear from a new line, add a newline at the end.
    /// </summary>
    /// <param name="buttons">List of buttons</param>
    public ReplyKeyboard(params string[] buttons)
        : base(GenerateMarkupList(buttons)) => ResizeKeyboard = true;
    
    /// <summary>
    /// Generates a markup list
    /// </summary>
    /// <param name="buttons">Button names</param>
    /// <returns>Markup list</returns>
    private static List<List<KeyboardButton>> GenerateMarkupList(string[] buttons) {
        var list = new List<List<KeyboardButton>>();
        var current = new List<KeyboardButton>();
        list.Add(current);
        foreach (var button in buttons) {
            current.Add(new KeyboardButton(button.TrimEnd('\n')));
            if (!button.EndsWith('\n')) continue;
            current = []; list.Add(current);
        }

        return list;
    }
}