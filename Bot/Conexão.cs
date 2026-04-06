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
    private GuildConfigSyncService _guildConfigSyncService = null!;

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

        _client.Ready += OnReady;
        _client.Log += Log;

        Console.WriteLine("Registrando eventos de entrada e saída...");
        _mongo = new MongoHandler(mongoSettings);
        _guildConfigSyncService = new GuildConfigSyncService(_mongo.GuildConfigService);

        _client.Ready += async () =>
        {
            Console.WriteLine("Bot pronto. Sincronizando guilds...");
            await _guildConfigSyncService.SyncAllGuildsAsync(_client);
        };

        var guildEvents = new GuildEvents(_client, _mongo);
        guildEvents.RegisterEvents();

        _messageHandler = new MessageHandler(_mongo);
        _client.MessageReceived += _messageHandler.HandleAsync;

        _voiceHandler = new VoiceHandler(_client, _mongo);
        _client.UserVoiceStateUpdated += _voiceHandler.HandleVoiceStateUpdatedAsync;

        _buttonHandler = new ButtonHandler(_mongo);
        _client.ButtonExecuted += _buttonHandler.HandleAsync;
        _client.SelectMenuExecuted += _buttonHandler.HandleAsync;

        _client.JoinedGuild += async guild =>
        {
            Console.WriteLine($"Entrou em novo servidor: {guild.Name}");
            await _guildConfigSyncService.SyncGuildAsync(guild);
        };

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
}