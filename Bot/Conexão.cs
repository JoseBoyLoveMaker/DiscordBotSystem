using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

class Bot
{
    private readonly IConfiguration _configuration;

    private DiscordSocketClient _client = null!;
    private MongoHandler _mongo = null!;
    private ButtonHandler _buttonHandler = null!;
    private VoiceHandler _voiceHandler = null!;
    private MessageHandler _messageHandler = null!;

    public Bot(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private async Task OnReady()
    {
        Console.WriteLine("Bot conectado! Carregando usuários...");

        foreach (var guild in _client.Guilds)
        {
            await guild.DownloadUsersAsync();
            Console.WriteLine($"✅ Cache carregado: {guild.Name}");
        }
    }

    public async Task StartAsync()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents =
                GatewayIntents.Guilds |
                GatewayIntents.GuildMessages |
                GatewayIntents.GuildMembers |
                GatewayIntents.MessageContent |
                GatewayIntents.GuildVoiceStates
        });

        var mongoSettings = _configuration.GetSection("Mongo").Get<MongoSettings>() ?? new MongoSettings();
        var discordSettings = _configuration.GetSection("Discord").Get<DiscordSettings>() ?? new DiscordSettings();

        if (string.IsNullOrWhiteSpace(discordSettings.Token))
            throw new Exception("Discord:Token não configurado.");

        _mongo = new MongoHandler(mongoSettings);

        _client.Ready += OnReady;
        _client.Log += Log;
        _client.UserJoined += OnUserJoined;
        _client.UserLeft += OnUserLeft;

        _messageHandler = new MessageHandler(_mongo);
        _client.MessageReceived += _messageHandler.HandleAsync;

        _voiceHandler = new VoiceHandler(_client, _mongo);
        _client.UserVoiceStateUpdated += _voiceHandler.HandleVoiceStateUpdatedAsync;

        _buttonHandler = new ButtonHandler(_mongo);
        _client.ButtonExecuted += _buttonHandler.HandleAsync;
        _client.SelectMenuExecuted += _buttonHandler.HandleAsync;

        await _client.LoginAsync(TokenType.Bot, discordSettings.Token);
        await _client.StartAsync();

        await _voiceHandler.DetectUsersAlreadyInVoiceAsync();

        try
        {
            await _mongo.BotStatusService.SetOnline();
            Console.WriteLine("Bot ONLINE no banco.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao marcar bot online: {ex.Message}");
        }

        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            try
            {
                _mongo.BotStatusService.SetOffline().GetAwaiter().GetResult();
                Console.WriteLine("Bot marcado como OFFLINE (ProcessExit).");
            }
            catch { }
        };

        Console.CancelKeyPress += (s, e) =>
        {
            _mongo.BotStatusService.SetOffline().GetAwaiter().GetResult();
            Console.WriteLine("Bot marcado como OFFLINE (CancelKeyPress).");
        };

        _voiceHandler.StartCallXpLoop();

        await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg);
        return Task.CompletedTask;
    }
    private async Task OnUserJoined(SocketGuildUser user)
    {
        Console.WriteLine($"{user.Username} entrou no servidor {user.Guild.Name}");

        // depois aqui puxar config do banco:
        // - mensagem de boas-vindas
        // - cargo automático
    }

    private async Task OnUserLeft(SocketGuild guild, SocketUser user)
    {
        Console.WriteLine($"{user.Username} saiu do servidor {guild.Name}");

        // depois aqui puxar config do banco:
        // - mensagem de saída personalizada
    }

}