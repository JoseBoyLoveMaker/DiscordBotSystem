using MongoDB.Driver;

public class MongoHandler
{
    private readonly IMongoDatabase _database;

    public IMongoCollection<UserData> Users { get; }
    public IMongoCollection<ResponseData> Responses { get; }
    public IMongoCollection<BotStatus> BotStatus { get; }
    public IMongoCollection<CommandConfig> Commands { get; }
    public IMongoCollection<EldenItem> EldenItems { get; }
    public IMongoCollection<GuildConfig> GuildConfig { get; }
    public IMongoCollection<GuildUserData> GuildUsers { get; }

    public UserService UserService { get; }
    public ResponseService ResponseService { get; }
    public BotStatusService BotStatusService { get; }
    public CommandService CommandService { get; }
    public GuildConfigService GuildConfigService { get; }
    public GuildUserService GuildUserService { get; }

    public MongoHandler(MongoSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);

        Users = _database.GetCollection<UserData>("users");
        Responses = _database.GetCollection<ResponseData>("responses");
        BotStatus = _database.GetCollection<BotStatus>("Stats");
        Commands = _database.GetCollection<CommandConfig>("commands");
        EldenItems = _database.GetCollection<EldenItem>("Elden");
        GuildConfig = _database.GetCollection<GuildConfig>("guildConfig");
        GuildUsers = _database.GetCollection<GuildUserData>("guild_users");

        UserService = new UserService(this);
        ResponseService = new ResponseService(this);
        BotStatusService = new BotStatusService(this);
        CommandService = new CommandService(this);
        GuildConfigService = new GuildConfigService(_database);
        GuildUserService = new GuildUserService(_database);
    }
}