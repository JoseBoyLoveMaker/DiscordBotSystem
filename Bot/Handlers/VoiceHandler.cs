using Discord.WebSocket;

public class VoiceHandler
{
    private readonly DiscordSocketClient _client;
    private readonly MongoHandler _mongo;

    private Dictionary<string, DateTime> usuariosEmCall = new();
    private Random rand = new Random();

    public VoiceHandler(
        DiscordSocketClient client,
        MongoHandler mongo)
    {
        _client = client;
        _mongo = mongo;
    }

    private string GetVoiceKey(ulong guildId, ulong userId)
    {
        return $"{guildId}_{userId}";
    }

    public async Task DetectUsersAlreadyInVoiceAsync()
    {
        foreach (var guild in _client.Guilds)
        {
            foreach (var channel in guild.VoiceChannels)
            {
                foreach (var user in channel.Users.Where(u => !u.IsBot))
                {
                    usuariosEmCall[GetVoiceKey(guild.Id, user.Id)] = DateTime.UtcNow;
                    Console.WriteLine($"Detectado no startup: {user.Username} está em call {channel.Name}");
                }
            }
        }
    }

    public Task HandleVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState before, SocketVoiceState after)
    {
        if (before.VoiceChannel == after.VoiceChannel)
            return Task.CompletedTask;

        var guild = (after.VoiceChannel ?? before.VoiceChannel)?.Guild;

        if (guild == null)
            return Task.CompletedTask;

        string key = GetVoiceKey(guild.Id, user.Id);

        if (before.VoiceChannel == null && after.VoiceChannel != null)
        {
            usuariosEmCall[key] = DateTime.UtcNow;
            Console.WriteLine($"{user.Username} entrou na call {after.VoiceChannel.Name}");
        }
        else if (before.VoiceChannel != null && after.VoiceChannel == null)
        {
            usuariosEmCall.Remove(key);
            Console.WriteLine($"{user.Username} saiu da call");
        }
        else if (before.VoiceChannel != after.VoiceChannel)
        {
            Console.WriteLine($"{user.Username} mudou de call {before.VoiceChannel.Name} -> {after.VoiceChannel.Name}");
        }

        return Task.CompletedTask;
    }

    public void StartCallXpLoop()
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Loop de Call XP rodando...");

                    var validGuilds = _client.Guilds;

                    foreach (var guild in validGuilds)
                    {
                        var usersInCall = guild.Users
                            .Where(u => u.VoiceChannel != null && !u.IsBot)
                            .GroupBy(u => u.VoiceChannel.Id);

                        foreach (var group in usersInCall)
                        {
                            var users = group.ToList();

                            if (users.Count < 2)
                                continue;

                            foreach (var user in users)
                            {
                                if (user.IsMuted && user.IsDeafened)
                                    continue;

                                int xp = rand.Next(8, 15);

                                await _mongo.GuildUserService.AddCallXp(
                                    guild.Id,
                                    user.Id,
                                    user.Username,
                                    xp
                                );

                                Console.WriteLine($"+{xp} CallXP para {user.Username} na call {user.VoiceChannel.Name} no servidor {guild.Name}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro no VoiceHandler / StartCallXpLoop:");
                    Console.WriteLine(ex);
                }

                await Task.Delay(60000);
            }
        });
    }
}