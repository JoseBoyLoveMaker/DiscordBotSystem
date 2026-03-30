using MongoDB.Driver;

public class BotStatusServiceAPI
{
    private readonly IMongoCollection<BotStatusAPI> _botStatus;

    public BotStatusServiceAPI(MongoHandlerAPI handler)
    {
        _botStatus = handler.BotStatus;
    }

    public async Task SetOnline()
    {
        await _botStatus.UpdateOneAsync(
            _ => true,
            Builders<BotStatusAPI>.Update
                .Set(x => x.IsOnline, true)
                .Set(x => x.LastUpdated, DateTime.UtcNow),
            new UpdateOptions { IsUpsert = true });
    }

    public async Task SetOffline()
    {
        await _botStatus.UpdateOneAsync(
            _ => true,
            Builders<BotStatusAPI>.Update
                .Set(x => x.IsOnline, false)
                .Set(x => x.LastUpdated, DateTime.UtcNow),
            new UpdateOptions { IsUpsert = true });
    }

    public async Task<BotStatusAPI?> GetStatus()
    {
        return await _botStatus.Find(_ => true).FirstOrDefaultAsync();
    }
}