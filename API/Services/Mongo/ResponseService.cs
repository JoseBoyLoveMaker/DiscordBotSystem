using MongoDB.Driver;

public class ResponseServiceAPI
{
    private readonly IMongoCollection<ResponseDataAPI> _responses;

    public ResponseServiceAPI(MongoHandlerAPI handler)
    {
        _responses = handler.Responses;
    }

    public async Task<List<ResponseDataAPI>> GetAllResponses()
    {
        return await _responses.Find(_ => true).ToListAsync();
    }

    public async Task<ResponseDataAPI?> GetByTrigger(string trigger)
    {
        return await _responses.Find(x => x.Trigger == trigger).FirstOrDefaultAsync();
    }

    public async Task AddResponse(string trigger, string nova)
    {
        var doc = await _responses.Find(x => x.Trigger == trigger).FirstOrDefaultAsync();

        if (doc == null)
        {
            var novoDoc = new ResponseDataAPI
            {
                Trigger = trigger,
                Responses = new List<string> { nova }
            };

            await _responses.InsertOneAsync(novoDoc);
            return;
        }

        var update = Builders<ResponseDataAPI>.Update.Push(x => x.Responses, nova);
        await _responses.UpdateOneAsync(x => x.Trigger == trigger, update);
    }

    public async Task DeleteResponse(string trigger, int index)
    {
        var doc = await _responses.Find(x => x.Trigger == trigger).FirstOrDefaultAsync();

        if (doc == null || index < 0 || index >= doc.Responses.Count)
            return;

        doc.Responses.RemoveAt(index);

        await _responses.ReplaceOneAsync(x => x.Trigger == trigger, doc);
    }

    public async Task EditResponse(string trigger, int index, string nova)
    {
        var doc = await _responses.Find(x => x.Trigger == trigger).FirstOrDefaultAsync();

        if (doc == null || index < 0 || index >= doc.Responses.Count)
            return;

        doc.Responses[index] = nova;

        await _responses.ReplaceOneAsync(x => x.Trigger == trigger, doc);
    }

    public async Task<string?> GetRandomResponse(string trigger)
    {
        var doc = await _responses.Find(x => x.Trigger == trigger).FirstOrDefaultAsync();

        if (doc == null || doc.Responses == null || doc.Responses.Count == 0)
            return null;

        var rand = new Random();
        return doc.Responses[rand.Next(doc.Responses.Count)];
    }
}