using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ResponseDataAPI
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("guildId")]
    public ulong GuildId { get; set; }

    [BsonElement("trigger")]
    public string Trigger { get; set; } = "";

    [BsonElement("responses")]
    public List<string> Responses { get; set; } = new();
}