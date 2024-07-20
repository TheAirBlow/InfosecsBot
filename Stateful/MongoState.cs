using MongoDB.Driver;
using Telegram.Bot.Types;

namespace Stateful;

/// <summary>
/// MongoDB implementation of MessageState storage.
/// No other option is available at the moment.
/// Database and collection names are hard-coded.
/// </summary>
internal static class MongoState {
    public static readonly IMongoCollection<MessageState> States;
    
    /// <summary>
    /// Initializes the MongoDB database
    /// </summary>
    static MongoState() {
        var client = new MongoClient(new MongoClientSettings {
            Server = new MongoServerAddress("localhost"),
            MaxConnectionPoolSize = 500
        });
        var database = client.GetDatabase("stateful");
        States = database.GetCollection<MessageState>("states");
    }
}