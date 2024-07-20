using System.Text;
using Humanizer;
using Stateful;
using Stateful.Attributes;
using Stateful.AutoKeyboard;

namespace InfosecsBot;

public class MainHandler : UpdateHandler {
    [DefaultHandler]
    private Task Default() => Task.CompletedTask;

    [Message("/obed@infosecs_obed_bot")]
    public async Task Obed() {
        var current = ObedManager.CurrentObed;
        if (current != null) {
            var endTime = (current.EndDate - ObedManager.CurrentTime).Humanize(precision: 2);
            await SendMessage($"🍽 Обэд кончится через {endTime} в {current.EndTime:hh\\:mm}");
            return;
        }
        
        var timeLeft = (ObedManager.ClosestObed - ObedManager.CurrentTime).Humanize(precision: 2);
        await SendMessage($"🍽 Следующий обэд будет через {timeLeft} в {ObedManager.ClosestObed:HH:mm}");
    }
    
    [Message("/list@infosecs_obed_bot")]
    public async Task List() {
        var builder = new StringBuilder();
        builder.Append("🍽 Список всех обэдов за день:\n\n");
        for (var i = 0; i < ObedManager.Obeds.Count; i++) {
            var obed = ObedManager.Obeds[i];
            var timeLeft = (obed.StartDate - ObedManager.CurrentTime).Humanize(precision: 2);
            builder.Append($"{i+1}. {obed.StartTime:hh\\:mm} - {obed.EndTime:hh\\:mm} (через {timeLeft})\n");
        }
        
        await SendMessage(builder.ToString());
    }
}