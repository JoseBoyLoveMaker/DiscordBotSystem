using MongoDB.Driver;

public class CommandService
{
    private readonly IMongoCollection<CommandConfig> _commands;

    public CommandService(MongoHandler handler)
    {
        _commands = handler.Commands;
    }

    public async Task<List<CommandConfig>> GetCommandsByServer(ulong guildId)
    {
        return await _commands
            .Find(x => x.GuildId == guildId)
            .ToListAsync();
    }

    public async Task<CommandConfig?> GetCommand(ulong guildId, string commandName)
    {
        commandName = commandName.ToLower();

        return await _commands
            .Find(x => x.GuildId == guildId && x.CommandName == commandName)
            .FirstOrDefaultAsync();
    }

    public async Task CreateOrUpdateCommand(CommandConfig config)
    {
        config.CommandName = config.CommandName.ToLower();
        config.Aliases = config.Aliases
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.ToLower())
            .Distinct()
            .ToList();

        var filter = Builders<CommandConfig>.Filter.Where(x =>
            x.GuildId == config.GuildId &&
            x.CommandName == config.CommandName);

        await _commands.ReplaceOneAsync(
            filter,
            config,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task SetEnabled(ulong guildId, string commandName, bool enabled)
    {
        commandName = commandName.ToLower();

        await _commands.UpdateOneAsync(
            x => x.GuildId == guildId && x.CommandName == commandName,
            Builders<CommandConfig>.Update.Set(x => x.Enabled, enabled));
    }

    public async Task UpdateAliases(ulong guildId, string commandName, List<string> aliases)
    {
        commandName = commandName.ToLower();

        aliases = aliases
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.ToLower())
            .Distinct()
            .ToList();

        await _commands.UpdateOneAsync(
            x => x.GuildId == guildId && x.CommandName == commandName,
            Builders<CommandConfig>.Update.Set(x => x.Aliases, aliases));
    }

    public async Task UpdateCooldown(ulong guildId, string commandName, int cooldownSeconds)
    {
        commandName = commandName.ToLower();

        await _commands.UpdateOneAsync(
            x => x.GuildId == guildId && x.CommandName == commandName,
            Builders<CommandConfig>.Update.Set(x => x.CooldownSeconds, cooldownSeconds));
    }

    public async Task<CommandConfig?> ResolveCommand(ulong guildId, string input)
    {
        input = input.ToLower();

        // 1) tenta achar primeiro no servidor local
        var localCommand = await _commands.Find(x =>
            x.GuildId == guildId &&
            (x.CommandName == input || x.Aliases.Contains(input))
        ).FirstOrDefaultAsync();

        if (localCommand != null)
            return localCommand;

        // 2) tenta achar no global
        var globalCommand = await _commands.Find(x =>
            x.GuildId == 0 &&
            (x.CommandName == input || x.Aliases.Contains(input))
        ).FirstOrDefaultAsync();

        if (globalCommand == null)
            return null;

        // 3) se achou no global, verifica se existe override local
        // pelo nome real do comando global
        var localOverride = await _commands.Find(x =>
            x.GuildId == guildId &&
            x.CommandName == globalCommand.CommandName
        ).FirstOrDefaultAsync();

        if (localOverride != null)
            return localOverride;

        // 4) se não houver override local, usa o global
        return globalCommand;
    }
}