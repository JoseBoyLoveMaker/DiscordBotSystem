using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class BotStatusAPI
{
    [BsonId]
    public ObjectId Id { get; set; }

    public bool IsOnline { get; set; }

    public DateTime LastUpdated { get; set; }
}