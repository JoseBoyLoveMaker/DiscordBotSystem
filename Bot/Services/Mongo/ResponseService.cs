using MongoDB.Driver;

public class ResponseService
{
    private readonly IMongoCollection<ResponseData> _responses;
    private readonly Random _random = new();

    public ResponseService(MongoHandler handler)
    {
        _responses = handler.Responses;
    }

    public async Task<string?> GetRandomResponse(ulong guildId, string trigger)
    {
        var docs = await _responses
            .Find(x => x.GuildId == guildId && x.Trigger == trigger)
            .ToListAsync();

        if (docs.Count == 0)
            return null;

        var todasResponses = docs
            .Where(x => x.Responses != null && x.Responses.Count > 0)
            .SelectMany(x => x.Responses)
            .ToList();

        if (todasResponses.Count == 0)
            return null;

        return todasResponses[_random.Next(todasResponses.Count)];
    }
}