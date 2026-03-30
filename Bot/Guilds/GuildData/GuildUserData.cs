using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class GuildUserData
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string? Id { get; set; }

    [BsonElement("guildId")]
    public ulong GuildId { get; set; }

    [BsonElement("userId")]
    public ulong UserId { get; set; }

    [BsonElement("userName")]
    public string UserName { get; set; } = string.Empty;

    [BsonElement("chatXp")]
    public int ChatXp { get; set; }

    [BsonElement("callXp")]
    public int CallXp { get; set; }

    [BsonElement("chatLevel")]
    public int ChatLevel { get; set; }

    [BsonElement("callLevel")]
    public int CallLevel { get; set; }
}