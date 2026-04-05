using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class GuildConfig
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string? Id { get; set; }

    [BsonElement("guildId")]
    public ulong GuildId { get; set; }

    [BsonElement("guildName")]
    public string GuildName { get; set; } = string.Empty;

    [BsonElement("commandsEnabled")]
    public bool CommandsEnabled { get; set; } = true;

    [BsonElement("eldenEnabled")]
    public bool EldenEnabled { get; set; } = true;

    [BsonElement("xpEnabled")]
    public bool XpEnabled { get; set; } = true;

    [BsonElement("commandsChannelId")]
    public ulong? CommandsChannelId { get; set; }

    [BsonElement("eldenChannelId")]
    public ulong? EldenChannelId { get; set; }

    [BsonElement("xpChannelId")]
    public ulong? XpChannelId { get; set; }

    [BsonElement("logChannelId")]
    public ulong? LogChannelId { get; set; }

    [BsonElement("prefix")]
    public string Prefix { get; set; } = "!";

    [BsonElement("welcome")]
    public WelcomeConfig Welcome { get; set; } = new();

    [BsonElement("leave")]
    public LeaveConfig Leave { get; set; } = new();

    [BsonElement("roles")]
    public RoleConfig Roles { get; set; } = new();
}