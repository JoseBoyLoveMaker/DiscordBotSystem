using MongoDB.Bson.Serialization.Attributes;

public class LevelRoleReward
{
    [BsonElement("roleId")]
    public ulong RoleId { get; set; }

    [BsonElement("minChatLevel")]
    public int MinChatLevel { get; set; }

    [BsonElement("minCallLevel")]
    public int MinCallLevel { get; set; }
}