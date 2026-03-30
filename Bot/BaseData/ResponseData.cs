using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ResponseData
{
    [BsonId]
    public ObjectId Id { get; set; }

    public string Trigger { get; set; } = "";

    public List<string> Responses { get; set; } = new();
}