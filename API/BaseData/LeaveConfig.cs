using MongoDB.Bson.Serialization.Attributes;

public class LeaveConfig
{
    [BsonElement("enabled")]
    public bool Enabled { get; set; }

    [BsonElement("channelId")]
    public ulong ChannelId { get; set; }

    [BsonElement("message")]
    public string Message { get; set; } = "{user} saiu do servidor.";
}