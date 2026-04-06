using MongoDB.Bson.Serialization.Attributes;

public class RoleConfig
{
    [BsonElement("levelRoles")]
    public List<LevelRoleReward> LevelRoles { get; set; } = new();

    [BsonElement("autoRoleId")]
    public ulong AutoRoleId { get; set; }
}