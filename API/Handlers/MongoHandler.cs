using MongoDB.Driver;

public class MongoHandlerAPI
{
    private readonly IMongoDatabase _database;

    public IMongoCollection<UserDataAPI> Users { get; }
    public IMongoCollection<ResponseDataAPI> Responses { get; }
    public IMongoCollection<BotStatusAPI> BotStatus { get; }
    public IMongoCollection<CommandConfigAPI> Commands { get; }

    public MongoHandlerAPI(MongoSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);

        Users = _database.GetCollection<UserDataAPI>("users");
        Responses = _database.GetCollection<ResponseDataAPI>("responses");
        BotStatus = _database.GetCollection<BotStatusAPI>("Stats");
        Commands = _database.GetCollection<CommandConfigAPI>("commands");
    }
}