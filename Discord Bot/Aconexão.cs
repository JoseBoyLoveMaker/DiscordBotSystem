using Discord;
using Discord.WebSocket;

class Aconexão
{
    private DiscordSocketClient _client;

    static Task Main(string[] args) => new Aconexão().MainAsync();

    public async Task MainAsync()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All
        });

        _client.Log += Log;
        _client.Ready += Ready;

        string token = "MTQ3OTI3OTM2OTQ3NzQyMzEzNQ.GXyHT9.3okdp9b8yQzwseQDPzt-Xbnl9qsgJrN41yZmn0";

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private Task Ready()
    {
        Console.WriteLine($"Bot conectado como {_client.CurrentUser}");
        return Task.CompletedTask;
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg);
        return Task.CompletedTask;
    }
}