using MongoDB.Driver;

public class BotStatusService
{
    private readonly IMongoCollection<BotStatus> _botStatus;

    public BotStatusService(MongoHandler handler)
    {
        _botStatus = handler.BotStatus;
    }

    // Método para definir o status do bot como online
    public async Task SetOnline()
    {
        await _botStatus.UpdateOneAsync(
            _ => true,
            Builders<BotStatus>.Update
                .Set(x => x.IsOnline, true)
                .Set(x => x.LastUpdated, DateTime.UtcNow),
            new UpdateOptions { IsUpsert = true });
    }

    // Método para definir o status do bot como offline
    public async Task SetOffline()
    {
        await _botStatus.UpdateOneAsync(
            _ => true,
            Builders<BotStatus>.Update
                .Set(x => x.IsOnline, false)
                .Set(x => x.LastUpdated, DateTime.UtcNow),
            new UpdateOptions { IsUpsert = true });
    }

    // Método para obter o status atual do bot
    public async Task<BotStatus?> GetStatus()
    {
        return await _botStatus.Find(_ => true).FirstOrDefaultAsync();
    }
}