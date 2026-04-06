using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class WelcomeConfig
{
    [BsonElement("enabled")]
    public bool Enabled { get; set; }

    [BsonElement("channelId")]
    public ulong? ChannelId { get; set; }

    [BsonElement("message")]
    public string Message { get; set; } = "Bem-vindo {user}!";
}