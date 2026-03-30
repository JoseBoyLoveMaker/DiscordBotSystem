using MongoDB.Driver;

public class GuildConfigService
{
    private readonly IMongoCollection<GuildConfig> _guildConfigs;

    public GuildConfigService(IMongoDatabase database)
    {
        _guildConfigs = database.GetCollection<GuildConfig>("guild_configs");
    }

    public async Task<GuildConfig> GetOrCreateConfig(ulong guildId, string? guildName = null)
    {
        var config = await _guildConfigs
            .Find(x => x.GuildId == guildId)
            .FirstOrDefaultAsync();

        if (config != null)
        {
            if (!string.IsNullOrWhiteSpace(guildName) && config.GuildName != guildName)
            {
                var update = Builders<GuildConfig>.Update.Set(x => x.GuildName, guildName);
                await _guildConfigs.UpdateOneAsync(x => x.GuildId == guildId, update);
                config.GuildName = guildName;
            }

            return config;
        }

        config = new GuildConfig
        {
            GuildId = guildId,
            GuildName = guildName ?? string.Empty
        };

        await _guildConfigs.InsertOneAsync(config);
        return config;
    }

    private bool IsCommandAllowed(ulong currentChannelId, ulong? generalChannelId, ulong? systemChannelId)
    {
        if (systemChannelId.HasValue && systemChannelId.Value != 0)
            return currentChannelId == systemChannelId.Value;

        if (generalChannelId.HasValue && generalChannelId.Value != 0)
            return currentChannelId == generalChannelId.Value;

        return true;
    }

    public async Task UpdateConfig(GuildConfig config)
    {
        await _guildConfigs.ReplaceOneAsync(
            x => x.GuildId == config.GuildId,
            config,
            new ReplaceOptions { IsUpsert = true }
        );
    }
}