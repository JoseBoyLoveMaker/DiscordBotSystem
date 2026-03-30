using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class UserDataAPI
{
    [BsonId]
    public ObjectId Id { get; set; }

    public ulong UserId { get; set; }

    public int ChatXp { get; set;} = 0;

    public int CallXp { get; set; } = 0;

    public int ChatLevel { get; set; } = 0;

    public int CallLevel { get; set; } = 0;

}
