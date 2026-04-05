using Discord.WebSocket;

public class GuildConfigSyncService
{
    private readonly GuildConfigService _guildConfigService;

    public GuildConfigSyncService(GuildConfigService guildConfigService)
    {
        _guildConfigService = guildConfigService;
    }

    public async Task SyncGuildAsync(SocketGuild guild)
    {
        var config = await _guildConfigService.GetOrCreateConfig(guild.Id, guild.Name);

        config.AvailableChannels = guild.TextChannels
            .OrderBy(c => c.Position)
            .Select(c => new GuildChannelInfo
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToList();

        config.AvailableRoles = guild.Roles
            .Where(r => !r.IsEveryone)
            .OrderByDescending(r => r.Position)
            .Select(r => new GuildRoleInfo
            {
                Id = r.Id,
                Name = r.Name
            })
            .ToList();

        await _guildConfigService.UpdateConfig(config);
    }

    public async Task SyncAllGuildsAsync(DiscordSocketClient client)
    {
        foreach (var guild in client.Guilds)
        {
            await SyncGuildAsync(guild);
        }
    }
}