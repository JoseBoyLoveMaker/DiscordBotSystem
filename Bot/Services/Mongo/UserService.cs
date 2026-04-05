using MongoDB.Driver;

public class UserService
{
    private readonly IMongoCollection<GuildUserData> _users;

    public UserService(MongoHandler handler)
    {
        _users = handler.GuildUsers;
    }

    // Obtém um usuário por guild + user
    public async Task<GuildUserData?> GetUser(ulong guildId, ulong userId)
    {
        return await _users
            .Find(x => x.GuildId == guildId && x.UserId == userId)
            .FirstOrDefaultAsync();
    }

    // Cria ou obtém usuário existente
    public async Task<GuildUserData> GetOrCreateUser(ulong guildId, ulong userId, string userName = "")
    {
        var user = await GetUser(guildId, userId);

        if (user != null)
            return user;

        user = new GuildUserData
        {
            GuildId = guildId,
            UserId = userId,
            UserName = userName,
            ChatXp = 0,
            CallXp = 0,
            ChatLevel = 0,
            CallLevel = 0
        };

        await _users.InsertOneAsync(user);
        return user;
    }

    // Adiciona XP de chat e retorna true se subiu de nível
    public async Task<bool> AddChatXp(ulong guildId, ulong userId, int xp)
    {
        var filter = Builders<GuildUserData>.Filter.Where(x => x.GuildId == guildId && x.UserId == userId);
        var user = await _users.Find(filter).FirstOrDefaultAsync();

        if (user == null)
            return false;

        int newXp = user.ChatXp + xp;
        int oldLevel = user.ChatLevel;
        int newLevel = CalculateLevel(newXp);

        bool levelUp = newLevel > oldLevel;

        var update = Builders<GuildUserData>.Update
            .Set(x => x.ChatXp, newXp)
            .Set(x => x.ChatLevel, newLevel);

        await _users.UpdateOneAsync(filter, update);
        return levelUp;
    }

    // Adiciona XP de call e retorna true se subiu de nível
    public async Task<bool> AddCallXp(ulong guildId, ulong userId, string userName, int xp)
    {
        var filter = Builders<GuildUserData>.Filter.Where(x => x.GuildId == guildId && x.UserId == userId);
        var user = await _users.Find(filter).FirstOrDefaultAsync();

        if (user == null)
            return false;

        int newXp = user.CallXp + xp;
        int oldLevel = user.CallLevel;
        int newLevel = CalculateLevel(newXp);

        bool levelUp = newLevel > oldLevel;

        var update = Builders<GuildUserData>.Update
            .Set(x => x.CallXp, newXp)
            .Set(x => x.CallLevel, newLevel);

        await _users.UpdateOneAsync(filter, update);
        return levelUp;
    }

    // Top de chat por servidor
    public async Task<List<GuildUserData>> GetTopChat(ulong guildId, int page, int pageSize)
    {
        return await _users
            .Find(x => x.GuildId == guildId)
            .SortByDescending(x => x.ChatXp)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    // Top de call por servidor
    public async Task<List<GuildUserData>> GetTopCall(ulong guildId, int page, int pageSize)
    {
        return await _users
            .Find(x => x.GuildId == guildId)
            .SortByDescending(x => x.CallXp)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    // Posição no ranking de chat por servidor
    public async Task<int> GetChatPosition(ulong guildId, ulong userId)
    {
        var user = await GetUser(guildId, userId);
        if (user == null) return -1;

        var count = await _users.CountDocumentsAsync(x => x.GuildId == guildId && x.ChatXp > user.ChatXp);
        return (int)count + 1;
    }

    // Posição no ranking de call por servidor
    public async Task<int> GetCallPosition(ulong guildId, ulong userId)
    {
        var user = await GetUser(guildId, userId);
        if (user == null) return -1;

        var count = await _users.CountDocumentsAsync(x => x.GuildId == guildId && x.CallXp > user.CallXp);
        return (int)count + 1;
    }

    // Cálculo de nível
    public int CalculateLevel(int totalXp)
    {
        int level = 0;
        while (totalXp >= GetRequiredXp(level))
        {
            totalXp -= GetRequiredXp(level);
            level++;
        }
        return level;
    }

    public int GetRequiredXp(int level)
    {
        return 5 * (level * level) + 50 * level + 100;
    }

    public int GetXpForLevel(int level)
    {
        int xp = 0;
        for (int i = 0; i < level; i++)
            xp += GetRequiredXp(i);

        return xp;
    }
}