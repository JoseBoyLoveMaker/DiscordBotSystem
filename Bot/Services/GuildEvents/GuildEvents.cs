using Discord.WebSocket;

public class GuildEvents
{
    private readonly DiscordSocketClient _client;
    private readonly MongoHandler _mongo;

    public GuildEvents(DiscordSocketClient client, MongoHandler mongo)
    {
        _client = client;
        _mongo = mongo;
    }

    public void RegisterEvents()
    {
        _client.UserJoined += OnUserJoined;
        _client.UserLeft += OnUserLeft;
    }

    private async Task OnUserJoined(SocketGuildUser user)
    {
        var config = await _mongo.GuildConfigService.GetOrCreateConfig(user.Guild.Id, user.Guild.Name);

        if (config?.Welcome == null || !config.Welcome.Enabled)
            return;

        if (!config.Welcome.ChannelId.HasValue)
            return;

        var channel = user.Guild.GetTextChannel(config.Welcome.ChannelId.Value);

        if (channel == null)
            return;

        var msg = (config.Welcome.Message ?? "Bem-vindo {user}!")
            .Replace("{user}", user.Mention)
            .Replace("{server}", user.Guild.Name)
            .Replace("{memberCount}", user.Guild.MemberCount.ToString());

        await channel.SendMessageAsync(msg);

        var roleId = config.Roles.AutoRoleId;

        if (config?.Roles?.AutoRoleId.HasValue == true)
        {
            var role = user.Guild.GetRole(config.Roles.AutoRoleId.Value);
            if (role != null)
            {
                await user.AddRoleAsync(role);
            }
        }
    }

    private async Task OnUserLeft(SocketGuild guild, SocketUser user)
    {
        var config = await _mongo.GuildConfigService.GetOrCreateConfig(guild.Id, guild.Name);

        if (config?.Leave == null || !config.Leave.Enabled)
            return;

        if (!config.Leave.ChannelId.HasValue)
            return;

        var channel = guild.GetTextChannel(config.Leave.ChannelId.Value);

        if (channel == null)
            return;

        var msg = (config.Leave.Message ?? "{user} saiu do servidor.")
            .Replace("{user}", user.Username);

        await channel.SendMessageAsync(msg);
    }
}