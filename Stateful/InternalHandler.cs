using Stateful.AutoKeyboard;
using Stateful.Keyboards;
using Telegram.Bot;

namespace Stateful;

/// <summary>
/// Handles internal stuff
/// </summary>
public class InternalHandler : UpdateHandler {
    [InternalCallback("paginator")]
    private async Task Paginator() {
        var id = Update.CallbackQuery?.Data;
        if (!int.TryParse(id?[21..], out var page)) return;
        var data = State.GetState<PaginatorKeyboard.PaginatorData>("paginator_data");
        if (data == null || page < 0 || page >= data.Pages || page == data.Page) return;
        data.Page = page; State.SetState("paginator_data", data);
        await Client.EditMessageReplyMarkupAsync(ChatId!.Value,
            MessageId!.Value, new InlineKeyboard(data.GetButtons()));
    }
}