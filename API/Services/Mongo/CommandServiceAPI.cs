using MongoDB.Driver;

public class CommandServiceAPI
{
    private readonly IMongoCollection<CommandConfigAPI> _commands;

    public CommandServiceAPI(MongoHandlerAPI handler)
    {
        _commands = handler.Commands;
    }

    public async Task<List<CommandConfigAPI>> GetCommandsByGuild(ulong guildId)
    {
        var all = await _commands
            .Find(x => x.GuildId == 0 || x.GuildId == guildId)
            .ToListAsync();

        // locais do servidor
        var locals = all
            .Where(x => x.GuildId == guildId)
            .ToDictionary(x => x.CommandName, x => x);

        // globais
        var globals = all
            .Where(x => x.GuildId == 0)
            .ToList();

        var result = new List<CommandConfigAPI>();

        
        foreach (var global in globals)
        {
            if (locals.TryGetValue(global.CommandName, out var localOverride))
                result.Add(localOverride);
            else
                result.Add(global);
        }

        
        foreach (var local in locals.Values)
        {
            if (!globals.Any(g => g.CommandName == local.CommandName))
                result.Add(local);
        }

        return result
            .OrderBy(x => x.CommandName)
            .ToList();
    }

    public async Task<CommandConfigAPI?> GetCommandByGuild(ulong guildId, string commandName)
    {
        commandName = commandName.ToLower();

        var local = await _commands
            .Find(x => x.GuildId == guildId && x.CommandName == commandName)
            .FirstOrDefaultAsync();

        if (local != null)
            return local;

        return await _commands
            .Find(x => x.GuildId == 0 && x.CommandName == commandName)
            .FirstOrDefaultAsync();
    }

    public async Task<CommandConfigAPI?> GetOrCreateLocalOverride(ulong guildId, string commandName)
    {
        commandName = commandName.ToLower();

        var local = await _commands
            .Find(x => x.GuildId == guildId && x.CommandName == commandName)
            .FirstOrDefaultAsync();

        if (local != null)
            return local;

        var global = await _commands
            .Find(x => x.GuildId == 0 && x.CommandName == commandName)
            .FirstOrDefaultAsync();

        if (global == null)
            return null;

        var clone = new CommandConfigAPI
        {
            GuildId = guildId,
            CommandName = global.CommandName,
            Description = global.Description,
            Enabled = global.Enabled,
            Aliases = new List<string>(global.Aliases),
            CooldownSeconds = global.CooldownSeconds,
            IsVip = global.IsVip
        };

        await _commands.InsertOneAsync(clone);
        return clone;
    }

    public async Task<bool> SetEnabled(ulong guildId, string commandName, bool enabled)
    {
        commandName = commandName.ToLower();

        var local = await GetOrCreateLocalOverride(guildId, commandName);
        if (local == null)
            return false;

        await _commands.UpdateOneAsync(
            x => x.GuildId == guildId && x.CommandName == commandName,
            Builders<CommandConfigAPI>.Update.Set(x => x.Enabled, enabled));

        return true;
    }

    public async Task<bool> UpdateAliases(ulong guildId, string commandName, List<string> aliases)
    {
        commandName = commandName.ToLower();

        aliases = aliases
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.ToLower())
            .Distinct()
            .ToList();

        var local = await GetOrCreateLocalOverride(guildId, commandName);
        if (local == null)
            return false;

        await _commands.UpdateOneAsync(
            x => x.GuildId == guildId && x.CommandName == commandName,
            Builders<CommandConfigAPI>.Update.Set(x => x.Aliases, aliases));

        return true;
    }

    public async Task<bool> UpdateCooldown(ulong guildId, string commandName, int cooldown)
    {
        commandName = commandName.ToLower();

        var local = await GetOrCreateLocalOverride(guildId, commandName);
        if (local == null)
            return false;

        await _commands.UpdateOneAsync(
            x => x.GuildId == guildId && x.CommandName == commandName,
            Builders<CommandConfigAPI>.Update.Set(x => x.CooldownSeconds, cooldown));

        return true;
    }
}