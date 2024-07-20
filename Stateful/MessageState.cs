using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Telegram.Bot.Types;

namespace Stateful;

/// <summary>
/// Message state information
/// </summary>
public class MessageState {
    /// <summary>
    /// Unique identifier of this message state
    /// </summary>
    [BsonId] public ObjectId StateId { get; set; }
    
    /// <summary>
    /// A dictionary of states you can use to store arbitrary information
    /// </summary>
    public Dictionary<string, string> State { get; set; }
    
    /// <summary>
    /// When was this message state last updated
    /// </summary>
    public DateTime LastUpdated { get; set; }
    
    /// <summary>
    /// Unique identifier of the current update handler
    /// </summary>
    public string? HandlerId { get; set; }
    
    /// <summary>
    /// Telegram message ID
    /// </summary>
    public int MessageId { get; set; }
    
    /// <summary>
    /// Telegram chat ID
    /// </summary>
    public long ChatId { get; set; }

    /// <summary>
    /// Changes Handler ID
    /// </summary>
    /// <param name="id">Handler ID</param>
    internal async Task SetHandler(string id) {
        LastUpdated = DateTime.Now;
        HandlerId = id; await ApplyChanges();
    }

    /// <summary>
    /// Get state value
    /// </summary>
    /// <param name="key">Dictionary Key</param>
    /// <typeparam name="T">Type</typeparam>
    /// <returns>Value of type</returns>
    public T? GetState<T>(string key)
        => !State.TryGetValue(key, out var value)
            ? default : JsonSerializer.Deserialize<T>(value);
    
    /// <summary>
    /// Remove state
    /// </summary>
    /// <param name="key">Dictionary Key</param>
    public void RemoveState(string key) {
        LastUpdated = DateTime.Now;
        State.Remove(key);
    }
    
    /// <summary>
    /// Set state value
    /// </summary>
    /// <param name="key">Dictionary Key</param>
    /// <param name="value">Dictionary Value</param>
    public void SetState(string key, object value) {
        State[key] = JsonSerializer.Serialize(value);
        LastUpdated = DateTime.Now;
    }
    
    /// <summary>
    /// Get last message state in a chat
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>Message State if found</returns>
    internal static async Task<MessageState> Get(Update update) {
        var chatId = update.GetChatId();
        var messageId = update.GetMessageId();
        
        // Try to find by Chat ID and Message ID
        var filter = new ExpressionFilterDefinition<MessageState>(
            x => x.ChatId == chatId && x.MessageId == messageId);
        using var res = await MongoState.States.FindAsync(filter,
            new FindOptions<MessageState> { Limit = 1 });
        await res.MoveNextAsync();
        var state1 = res.Current.FirstOrDefault();
        if (state1 != null) return state1;
        
        // Try to find last message state in chat
        var filter2 = new ExpressionFilterDefinition<MessageState>(x => x.ChatId == chatId);
        var sort = Builders<MessageState>.Sort.Descending(x => x.StateId);
        using var res2 = await MongoState.States.FindAsync(filter2,
            new FindOptions<MessageState> { Limit = 1, Sort = sort });
        await res2.MoveNextAsync();
        var state2 = res2.Current.FirstOrDefault();
        if (state2 != null) return state2;
        
        // Fallback to creating a new one
        var state3 = new MessageState {
            LastUpdated = DateTime.Now, State = new Dictionary<string, string>(),
            MessageId = messageId!.Value, ChatId = chatId!.Value
        };

        await MongoState.States.InsertOneAsync(state3);
        return state3;
    }

    /// <summary>
    /// Applies all changes to this object
    /// </summary>
    public async Task ApplyChanges() {
        var filter = Builders<MessageState>.Filter.Eq(x => x.StateId, StateId);
        await MongoState.States.FindOneAndReplaceAsync(filter, this);
    }
}