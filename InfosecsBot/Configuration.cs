using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;

namespace InfosecsBot; 

/// <summary>
/// Global configuration file
/// </summary>
public class Configuration {
    /// <summary>
    /// JSON serializer options
    /// </summary>
    public static readonly JsonSerializerOptions Options = new() {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = true
    };
    
    /// <summary>
    /// Static object instance
    /// </summary>
    public static Configuration Config;

    /// <summary>
    /// Load the configuration
    /// </summary>
    static Configuration() {
        if (File.Exists("config.json")) {
            Log.Information("Loading configuration file...");
            var content = File.ReadAllText("config.json");
            try {
                Config = JsonSerializer.Deserialize<Configuration>(content, Options)!;
            } catch (Exception e) {
                Log.Fatal("Failed to load config: {0}", e);
                Environment.Exit(-1);
            }
            return;
        }

        Config = new Configuration(); Config.Save();
        Log.Fatal("Configuration file doesn't exist, created a new one!");
        Log.Fatal("Please fill it with all the necessary information.");
        Environment.Exit(-1);
    }
    
    /// <summary>
    /// Period of time to wait before checking obed
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Next obed check
    /// </summary>
    public DateTime NextCheck { get; set; } = ObedManager.CurrentTime;
    
    /// <summary>
    /// Telegram bot token
    /// </summary>
    public string BotToken { get; set; } = "change_me";

    /// <summary>
    /// List of allowed groups
    /// </summary>
    public List<long> Groups { get; set; } = [];

    /// <summary>
    /// Current timezone (hour offset)
    /// </summary>
    public int Timezone { get; set; } = 3;

    /// <summary>
    /// Save configuration changes
    /// </summary>
    public void Save() => File.WriteAllText("config.json", 
        JsonSerializer.Serialize(Config, Options));
}