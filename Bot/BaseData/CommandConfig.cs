using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class CommandConfig
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("guildId")]
    public ulong GuildId { get; set; }

    [BsonElement("commandName")]
    public string CommandName { get; set; } = "";

    [BsonElement("description")]
    public string Description { get; set; } = "";

    [BsonElement("enabled")]
    public bool Enabled { get; set; } = true;

    [BsonElement("aliases")]
    public List<string> Aliases { get; set; } = new();

    [BsonElement("cooldownSeconds")]
    public int CooldownSeconds { get; set; } = 0;

    [BsonElement("isVip")]
    public bool IsVip { get; set; } = false;
}