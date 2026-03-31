using MongoDB.Driver;

public class ResponseServiceAPI
{
    private readonly IMongoCollection<ResponseDataAPI> _responses;

    public ResponseServiceAPI(MongoHandlerAPI mongo)
    {
        _responses = mongo.Responses;
    }

    public async Task<List<ResponseDataAPI>> GetAllResponses(ulong guildId)
    {
        return await _responses
            .Find(x => x.GuildId == guildId)
            .SortBy(x => x.Trigger)
            .ToListAsync();
    }

    public async Task<ResponseDataAPI?> GetByTrigger(ulong guildId, string trigger)
    {
        return await _responses
            .Find(x => x.GuildId == guildId && x.Trigger == trigger)
            .FirstOrDefaultAsync();
    }

    public async Task CreateTrigger(ulong guildId, string trigger)
    {
        var exists = await _responses
            .Find(x => x.GuildId == guildId && x.Trigger == trigger)
            .AnyAsync();

        if (exists)
            return;

        var data = new ResponseDataAPI
        {
            GuildId = guildId,
            Trigger = trigger,
            Responses = new List<string>()
        };

        await _responses.InsertOneAsync(data);
    }

    public async Task AddResponse(ulong guildId, string trigger, string nova)
    {
        var data = await GetByTrigger(guildId, trigger);

        if (data == null)
        {
            data = new ResponseDataAPI
            {
                GuildId = guildId,
                Trigger = trigger,
                Responses = new List<string> { nova }
            };

            await _responses.InsertOneAsync(data);
            return;
        }

        data.Responses.Add(nova);

        await _responses.ReplaceOneAsync(
            x => x.Id == data.Id,
            data
        );
    }

    public async Task EditResponse(ulong guildId, string trigger, int index, string nova)
    {
        var data = await GetByTrigger(guildId, trigger);

        if (data == null)
            return;

        if (index < 0 || index >= data.Responses.Count)
            return;

        data.Responses[index] = nova;

        await _responses.ReplaceOneAsync(
            x => x.Id == data.Id,
            data
        );
    }

    public async Task DeleteResponse(ulong guildId, string trigger, int index)
    {
        var data = await GetByTrigger(guildId, trigger);

        if (data == null)
            return;

        if (index < 0 || index >= data.Responses.Count)
            return;

        data.Responses.RemoveAt(index);

        await _responses.ReplaceOneAsync(
            x => x.Id == data.Id,
            data
        );
    }

    public async Task DeleteTrigger(ulong guildId, string trigger)
    {
        await _responses.DeleteOneAsync(x =>
            x.GuildId == guildId &&
            x.Trigger == trigger
        );
    }
}