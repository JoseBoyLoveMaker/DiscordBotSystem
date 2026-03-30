using MongoDB.Driver;

public class UserServiceAPI
{
    private readonly IMongoCollection<UserDataAPI> _users;

    public UserServiceAPI(MongoHandlerAPI handler)
    {
        _users = handler.Users;
    }

    // Método para obter um usuário pelo ID
    public async Task<UserDataAPI?> GetUser(ulong userId)
    {
        return await _users.Find(x => x.UserId == userId).FirstOrDefaultAsync();
    }

    // Método para criar um novo usuário ou obter um existente
    public async Task<UserDataAPI> GetOrCreateUser(ulong userId)
    {
        var user = await GetUser(userId);

        if (user != null)
            return user;

        user = new UserDataAPI
        {
            UserId = userId,
            ChatXp = 0,
            CallXp = 0,
            ChatLevel = 0,
            CallLevel = 0
        };

        // Insere o novo usuário no banco de dados

        await _users.InsertOneAsync(user);
        return user;
    }

    // Adiciona XP de chat e retorna true se subiu de nível
    public async Task<bool> AddChatXp(ulong userId, int xp)
    {
        var filter = Builders<UserDataAPI>.Filter.Eq(x => x.UserId, userId);
        var user = await _users.Find(filter).FirstOrDefaultAsync();

        if (user == null) return false;

        int newXp = user.ChatXp + xp;
        int oldLevel = user.ChatLevel;
        int newLevel = CalculateLevel(newXp);

        bool levelUp = newLevel > oldLevel;

        var update = Builders<UserDataAPI>.Update
            .Set(x => x.ChatXp, newXp)
            .Set(x => x.ChatLevel, newLevel);

        await _users.UpdateOneAsync(filter, update);
        return levelUp;
    }

    // Adiciona XP de call e retorna true se subiu de nível
    public async Task<bool> AddCallXp(ulong userId, int xp)
    {
        var filter = Builders<UserDataAPI>.Filter.Eq(x => x.UserId, userId);
        var user = await _users.Find(filter).FirstOrDefaultAsync();

        if (user == null) return false;

        int newXp = user.CallXp + xp;
        int oldLevel = user.CallLevel;
        int newLevel = CalculateLevel(newXp);

        bool levelUp = newLevel > oldLevel;

        var update = Builders<UserDataAPI>.Update
            .Set(x => x.CallXp, newXp)
            .Set(x => x.CallLevel, newLevel);

        await _users.UpdateOneAsync(filter, update);
        return levelUp;
    }

    // Método para obter o top de chat
    public async Task<List<UserDataAPI>> GetTopChat(int page, int pageSize)
    {
        return await _users
            .Find(_ => true)
            .SortByDescending(x => x.ChatXp)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    // Método para obter o top de call
    public async Task<List<UserDataAPI>> GetTopCall(int page, int pageSize)
    {
        return await _users
            .Find(_ => true)
            .SortByDescending(x => x.CallXp)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    // Método para obter a posição de um usuário no ranking de chat
    public async Task<int> GetChatPosition(ulong userId)
    {
        var user = await GetUser(userId);
        if (user == null) return -1;
        var count = await _users.CountDocumentsAsync(x => x.ChatXp > user.ChatXp);
        return (int)count + 1;
    }

    // Método para obter a posição de um usuário no ranking de call
    public async Task<int> GetCallPosition(ulong userId)
    {
        var user = await GetUser(userId);
        if (user == null) return -1;
        var count = await _users.CountDocumentsAsync(x => x.CallXp > user.CallXp);
        return (int)count + 1;
    }

    // Helper para calcular nível (mesma lógica do serviço anterior)
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

    // Retorna a soma de XP necessária até o início do nível informado
    public int GetXpForLevel(int level)
    {
        int xp = 0;
        for (int i = 0; i < level; i++)
            xp += GetRequiredXp(i);
        return xp;
    }

    // Método para contar o total de usuários
    public async Task<long> CountUsers()
    {
        return await _users.CountDocumentsAsync(_ => true);
    }

    // Método para calcular o total de XP acumulado por todos os usuários
    public async Task<int> TotalXp()
    {
        var users = await _users.Find(_ => true).ToListAsync();
        return users.Sum(u => u.ChatXp + u.CallXp);
    }
}