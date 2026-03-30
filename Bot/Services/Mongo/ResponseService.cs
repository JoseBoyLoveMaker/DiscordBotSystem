using MongoDB.Driver;

public class ResponseService
{
    private readonly IMongoCollection<ResponseData> _responses;

    public ResponseService(MongoHandler handler)
    {
        _responses = handler.Responses;
    }

    // Método para obter uma resposta aleatória com base em um gatilho
    public async Task<string?> GetRandomResponse(string trigger)
    {
        var doc = await _responses.Find(x => x.Trigger == trigger).FirstOrDefaultAsync();

        if (doc == null || doc.Responses == null || doc.Responses.Count == 0)
            return null;

        var rand = new Random();
        return doc.Responses[rand.Next(doc.Responses.Count)];
    }
}