using Discord;
using Discord.WebSocket;

public class RankCommand
{
    private readonly MongoHandler _mongo;

    public RankCommand(MongoHandler mongo)
    {
        _mongo = mongo;
    }

    public async Task ExecuteAsync(SocketMessage message)
    {
        if (message.Channel is not SocketGuildChannel guildChannel)
            return;

        var msg = message as SocketUserMessage;

        SocketUser targetUser;

        if (msg != null && msg.MentionedUsers.Count > 0)
            targetUser = msg.MentionedUsers.First();
        else
            targetUser = message.Author;

        ulong guildId = guildChannel.Guild.Id;

        var user = await _mongo.GuildUserService.GetUser(guildId, targetUser.Id);

        if (user == null)
        {
            await message.Channel.SendMessageAsync("Usuário não encontrado no ranking deste servidor.");
            return;
        }

        int chatLevel = _mongo.GuildUserService.CalculateLevel(user.ChatXp);
        int chatXpIntoLevel = user.ChatXp - _mongo.GuildUserService.GetXpForLevel(chatLevel);
        int chatXpRequired = _mongo.GuildUserService.GetRequiredXp(chatLevel);

        int callLevel = _mongo.GuildUserService.CalculateLevel(user.CallXp);
        int callXpIntoLevel = user.CallXp - _mongo.GuildUserService.GetXpForLevel(callLevel);
        int callXpRequired = _mongo.GuildUserService.GetRequiredXp(callLevel);

        int chatPosition = await _mongo.GuildUserService.GetChatPosition(guildId, targetUser.Id);
        int callPosition = await _mongo.GuildUserService.GetCallPosition(guildId, targetUser.Id);

        var embed = new EmbedBuilder()
            .WithTitle($"📊 Rank de {targetUser.Username}")
            .AddField("Posição Chat", chatPosition <= 0 ? "-" : $"#{chatPosition}", true)
            .AddField("Posição Call", callPosition <= 0 ? "-" : $"#{callPosition}", true)
            .AddField("Chat Level", chatLevel, true)
            .AddField("Call Level", callLevel, true)
            .AddField("Chat XP", $"{chatXpIntoLevel}/{chatXpRequired}", true)
            .AddField("Call XP", $"{callXpIntoLevel}/{callXpRequired}", true)
            .WithColor(Color.Blue)
            .Build();

        await message.Channel.SendMessageAsync(embed: embed);
    }
}