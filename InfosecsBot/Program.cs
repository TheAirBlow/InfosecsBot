using static InfosecsBot.Configuration;
using System.Globalization;
using Humanizer;
using InfosecsBot;
using Serilog;
using Stateful;
using Telegram.Bot;

public static class Program {
    public static TelegramBotClient Client { get; private set; }
    
    public static async Task Main(string[] args) {
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("ru_RU");
        CultureInfo.CurrentUICulture = new CultureInfo("ru_RU");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console().CreateLogger();
        Log.Information("Starting Infosecs Obed Bot");
        Config.Save();

        Log.Information("Setting up the Telegram bot");
        Client = new TelegramBotClient(Config.BotToken);
        var stateful = new StatefulBot(Client);
        stateful.Register<MainHandler>("main");
        Client.StartReceiving(
            async (_, update, _) => await stateful.HandleUpdate(update), 
            (_, e, _) => Log.Error("Failed to receive: {0}", e));

        Log.Information("Starting background threads");
        new Thread(async () => await NotificationThread()).Start();

        Log.Information("Bot is now running");
        await Task.Delay(-1);
    }

    private static async Task NotificationThread() {
        try {
            while (true) {
                if (Config.NextCheck > DateTime.UtcNow)
                    await Task.Delay(Config.NextCheck - DateTime.UtcNow);
                if (ObedManager.CurrentObed != null) {
                    await SendMessage("🍽 ПРОИЗОШЕЛ ОБЭД!!1! Все срочно идите жрать в столовку!");
                    Config.NextCheck = ObedManager.CurrentTime + ObedManager.CurrentObed.EndTime;
                    continue;
                }

                var closest = ObedManager.ClosestObed;
                var diff = closest - ObedManager.CurrentTime;
                if (diff > TimeSpan.FromMinutes(30)) {
                    Config.NextCheck = DateTime.UtcNow + Config.Interval;
                    continue;
                }

                var timeLeft = (closest - ObedManager.CurrentTime).Humanize(precision: 2);
                await SendMessage($"🍽 СКОРО ОБЭД!!1! Будет через {timeLeft} в {closest:HH:mm}");
                Config.NextCheck = DateTime.UtcNow + (TimeSpan.FromMinutes(10) > diff ? Config.Interval / 2 : Config.Interval);
            }
        } catch (Exception e) {
            Log.Error("Notification thread crashed: {0}", e);
        }
    }

    private static async Task SendMessage(string msg) {
        foreach (var id in Config.Groups)
            try {
                await Client.SendTextMessageAsync(id, msg);
            } catch (Exception e) {
                Log.Error("Failed to send message: {0}", e);
            }
    }
}