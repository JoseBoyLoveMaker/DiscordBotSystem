using Discord;
using Discord.WebSocket;
using System.Text.RegularExpressions;

public class MessageHandler
{
    private readonly MongoHandler _mongo;

    private readonly TopCommand _topCommand;
    private readonly RankCommand _rankCommand;
    private readonly RollCommand _rollCommand;
    private readonly MathCommand _mathCommand;
    private readonly EldenCommand _eldenCommand;

    private readonly Dictionary<string, DateTime> chatCooldown = new();
    private readonly Random rand = new();

    public MessageHandler(MongoHandler mongo)
    {
        _mongo = mongo;

        _topCommand = new TopCommand(_mongo);
        _rankCommand = new RankCommand(_mongo);
        _rollCommand = new RollCommand();
        _mathCommand = new MathCommand();
        _eldenCommand = new EldenCommand(_mongo);
    }

    private string GetChatKey(ulong guildId, ulong userId)
    {
        return $"{guildId}_{userId}";
    }

    public async Task HandleAsync(SocketMessage message)
    {
        try
        {
            if (message.Author.IsBot)
                return;

            if (message.Channel is not SocketGuildChannel guildChannel)
                return;

            var guild = guildChannel.Guild;

            string content = message.Content.Trim();

            if (string.IsNullOrWhiteSpace(content))
                return;

            var parts = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return;

            string firstWord = parts[0].ToLower();
            string commandInput = firstWord.StartsWith("!") ? firstWord.Substring(1) : firstWord;

            Console.WriteLine($"Mensagem detectada: {message.Author.Username} -> {content}");

            ulong guildId = guild.Id;
            ulong userId = message.Author.Id;
            string userName = message.Author.Username;
            var guildUserSocket = guild.GetUser(userId);

            await _mongo.GuildUserService.GetOrCreateUser(guildId, userId, userName);

            CommandConfig? resolvedCommand = await _mongo.CommandService.ResolveCommand(guildId, commandInput);

            Console.WriteLine($"[CMD] input: {commandInput}");
            Console.WriteLine($"[CMD] resolved: {(resolvedCommand == null ? "null" : resolvedCommand.CommandName)}");
            Console.WriteLine($"[CMD] resolved guildId: {(resolvedCommand == null ? "null" : resolvedCommand.GuildId)}");
            Console.WriteLine($"[CMD] enabled: {(resolvedCommand == null ? "null" : resolvedCommand.Enabled)}");

            // Se existir no banco, usa o nome real do comando.
            // Se não existir, usa o que o usuário digitou como fallback.
            string commandName = resolvedCommand?.CommandName?.ToLower() ?? commandInput;

            Console.WriteLine($"[CMD] final commandName: {commandName}");

            // Se o comando existir no banco e estiver desativado, bloqueia.
            if (resolvedCommand != null && !resolvedCommand.Enabled)
            {
                await message.Channel.SendMessageAsync("Esse comando está desativado neste servidor.");
                return;
            }

            switch (commandName)
            {
                case "top":
                    await _topCommand.ExecuteTopAsync(message, guild);
                    return;

                case "toptxt":
                    await _topCommand.ExecuteTopTxtAsync(message, guild);
                    return;

                case "topvoz":
                    await _topCommand.ExecuteTopVozAsync(message, guild);
                    return;

                case "rank":
                    await _rankCommand.ExecuteAsync(message);
                    return;

                case "elden":
                    await _eldenCommand.ExecuteAsync(message);
                    return;

                case "roll":
                    {
                        string expr = content.Substring(parts[0].Length).Trim();

                        if (string.IsNullOrWhiteSpace(expr))
                        {
                            await message.Channel.SendMessageAsync("Use o comando com uma expressão. Exemplo: `!roll 1d20+5`");
                            return;
                        }

                        if (Regex.IsMatch(expr.ToLower(), @"^[0-9d+\-*/().#!^z%a\s#]+$"))
                        {
                            await _rollCommand.ExecuteAsync(message);
                            return;
                        }

                        await message.Channel.SendMessageAsync("Expressão de dado inválida.");
                        return;
                    }

                case "math":
                    {
                        string expr = content.Substring(parts[0].Length).Trim();

                        if (string.IsNullOrWhiteSpace(expr))
                        {
                            await message.Channel.SendMessageAsync("Use o comando com uma expressão. Exemplo: `!math 2+2*5`");
                            return;
                        }

                        if (Regex.IsMatch(expr.ToLower(), @"^[0-9+\-*/().^z%\s]+$"))
                        {
                            await _mathCommand.ExecuteAsync(message);
                            return;
                        }

                        await message.Channel.SendMessageAsync("Expressão matemática inválida.");
                        return;
                    }
            }

            if (content.Length >= 5 && content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 2)
            {
                string chatKey = GetChatKey(guildId, userId);

                if (!chatCooldown.TryGetValue(chatKey, out DateTime cooldown) || cooldown < DateTime.UtcNow)
                {
                    int xp = rand.Next(15, 26);

                    bool levelUp = await _mongo.GuildUserService.AddChatXp(guildId, userId, userName, xp);

                    chatCooldown[chatKey] = DateTime.UtcNow.AddSeconds(30);

                    Console.WriteLine($"+{xp} ChatXP para {userName} no servidor {guild.Name}");

                    if (levelUp)
                    {
                        var guildUser = await _mongo.GuildUserService.GetOrCreateUser(guildId, userId, userName);

                        await message.Channel.SendMessageAsync(
                            $"🎉 {message.Author.Mention} subiu para o nível **{guildUser.ChatLevel}** de chat!"
                        );

                        var config = await _mongo.GuildConfigService.GetOrCreateConfig(guild.Id, guild.Name);

                        if (guildUserSocket != null)
                        {
                            await CheckLevelRoles(guildUserSocket, guildUser, config);
                        }
                    }
                }
            }

            var reply = await _mongo.ResponseService.GetRandomResponse(guildId, content);

            if (reply != null)
                await message.Channel.SendMessageAsync(reply);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro no MessageHandler:");
            Console.WriteLine(ex);
        }
    }

    private async Task CheckLevelRoles(SocketGuildUser user, GuildUserData data, GuildConfig config)
    {
        if (config.Roles?.LevelRoles == null || config.Roles.LevelRoles.Count == 0)
            return;

        foreach (var reward in config.Roles.LevelRoles)
        {
            var role = user.Guild.GetRole(reward.RoleId);

            if (role == null)
                continue;

            bool qualifies =
                data.ChatLevel >= reward.MinChatLevel &&
                data.CallLevel >= reward.MinCallLevel;

            if (qualifies)
            {
                if (!user.Roles.Any(r => r.Id == role.Id))
                    await user.AddRoleAsync(role);
            }
            else
            {
                if (user.Roles.Any(r => r.Id == role.Id))
                    await user.RemoveRoleAsync(role);
            }
        }
    }
}