using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class RoleConfig
{
    [BsonElement("autoRoleId")]
    public ulong? AutoRoleId { get; set; }

    [BsonElement("levelRoles")]
    public List<LevelRoleReward> LevelRoleRewards { get; set; } = new();
}