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
        return await _commands
            .Find(x => x.GuildId == guildId)
            .ToListAsync();
    }

    public async Task SetEnabled(ulong guildId, string commandName, bool enabled)
    {
        commandName = commandName.ToLower();

        await _commands.UpdateOneAsync(
            x => x.GuildId == guildId && x.CommandName == commandName,
            Builders<CommandConfigAPI>.Update.Set(x => x.Enabled, enabled));
    }

    public async Task UpdateAliases(ulong guildId, string commandName, List<string> aliases)
    {
        commandName = commandName.ToLower();

        aliases = aliases
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.ToLower())
            .Distinct()
            .ToList();

        await _commands.UpdateOneAsync(
            x => x.GuildId == guildId && x.CommandName == commandName,
            Builders<CommandConfigAPI>.Update.Set(x => x.Aliases, aliases));
    }

    public async Task UpdateCooldown(ulong guildId, string commandName, int cooldown)
    {
        commandName = commandName.ToLower();

        await _commands.UpdateOneAsync(
            x => x.GuildId == guildId && x.CommandName == commandName,
            Builders<CommandConfigAPI>.Update.Set(x => x.CooldownSeconds, cooldown));
    }
}