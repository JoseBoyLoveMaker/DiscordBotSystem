using MongoDB.Driver;

public class GuildUserService
{
    private readonly IMongoCollection<GuildUserData> _guildUsers;

    public GuildUserService(IMongoDatabase database)
    {
        _guildUsers = database.GetCollection<GuildUserData>("guild_users");
    }

    public async Task<GuildUserData> GetOrCreateUser(ulong guildId, ulong userId, string userName)
    {
        var user = await _guildUsers.Find(x =>
            x.GuildId == guildId &&
            x.UserId == userId
        ).FirstOrDefaultAsync();

        if (user != null)
            return user;

        user = new GuildUserData
        {
            GuildId = guildId,
            UserId = userId,
            UserName = userName
        };

        await _guildUsers.InsertOneAsync(user);
        return user;
    }

    public async Task<GuildUserData?> GetUser(ulong guildId, ulong userId)
    {
        return await _guildUsers.Find(x =>
            x.GuildId == guildId &&
            x.UserId == userId
        ).FirstOrDefaultAsync();
    }

    public async Task UpdateUser(GuildUserData user)
    {
        await _guildUsers.ReplaceOneAsync(
            x => x.GuildId == user.GuildId && x.UserId == user.UserId,
            user
        );
    }

    public async Task<bool> AddCallXp(ulong guildId, ulong userId, string userName, int xp)
    {
        var user = await GetOrCreateUser(guildId, userId, userName);

        user.CallXp += xp;

        int oldLevel = user.CallLevel;
        int newLevel = CalculateLevel(user.CallXp);

        user.CallLevel = newLevel;

        await _guildUsers.ReplaceOneAsync(
            x => x.GuildId == guildId && x.UserId == userId,
            user
        );

        return newLevel > oldLevel;
    }

    public async Task<bool> AddChatXp(ulong guildId, ulong userId, string userName, int xp)
    {
        var user = await GetOrCreateUser(guildId, userId, userName);

        user.ChatXp += xp;

        int oldLevel = user.ChatLevel;
        int newLevel = CalculateLevel(user.ChatXp);

        user.ChatLevel = newLevel;

        await _guildUsers.ReplaceOneAsync(
            x => x.GuildId == guildId && x.UserId == userId,
            user
        );

        return newLevel > oldLevel;
    }

    public async Task<List<GuildUserData>> GetTopChat(ulong guildId, int page, int pageSize)
    {
        return await _guildUsers
            .Find(x => x.GuildId == guildId)
            .SortByDescending(x => x.ChatXp)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<List<GuildUserData>> GetTopCall(ulong guildId, int page, int pageSize)
    {
        return await _guildUsers
            .Find(x => x.GuildId == guildId)
            .SortByDescending(x => x.CallXp)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetChatPosition(ulong guildId, ulong userId)
    {
        var user = await GetUser(guildId, userId);
        if (user == null) return -1;

        var count = await _guildUsers.CountDocumentsAsync(x =>
            x.GuildId == guildId &&
            x.ChatXp > user.ChatXp
        );

        return (int)count + 1;
    }

    public async Task<int> GetCallPosition(ulong guildId, ulong userId)
    {
        var user = await GetUser(guildId, userId);
        if (user == null) return -1;

        var count = await _guildUsers.CountDocumentsAsync(x =>
            x.GuildId == guildId &&
            x.CallXp > user.CallXp
        );

        return (int)count + 1;
    }

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