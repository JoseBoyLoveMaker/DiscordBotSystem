using MongoDB.Bson.Serialization.Attributes;

public class RoleConfig
{
    [BsonElement("autoRoleId")]
    public ulong? AutoRoleId { get; set; }

    [BsonElement("levelRoleRewards")]
    public List<LevelRoleReward> LevelRoleRewards { get; set; } = new();
}