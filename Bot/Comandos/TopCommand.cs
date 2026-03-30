using Discord;
using Discord.WebSocket;
using System.Text;

public class TopCommand
{
    private readonly MongoHandler _mongo;

    public TopCommand(MongoHandler mongo)
    {
        _mongo = mongo;
    }

    public async Task ExecuteTopAsync(SocketMessage message, SocketGuild guild)
    {
        int page = 1;
        int pageSize = 10;

        var topUsers = await _mongo.GuildUserService.GetTopChat(guild.Id, page, pageSize);

        if (topUsers == null || topUsers.Count == 0)
        {
            await message.Channel.SendMessageAsync("Ainda não há usuários no ranking deste servidor.");
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle("🏆 Ranking de Chat XP")
            .WithColor(Color.Gold);

        var ranking = new StringBuilder();
        int position = 1;

        foreach (var user in topUsers)
        {
            string name = guild.GetUser(user.UserId)?.DisplayName ?? user.UserName;

            string medal = position switch
            {
                1 => "🥇",
                2 => "🥈",
                3 => "🥉",
                _ => $"`#{position}`"
            };

            ranking.AppendLine($"{medal} **{name}** — `{user.ChatXp:N0} XP`");
            position++;
        }

        string rankingText = ranking.ToString();
        if (string.IsNullOrWhiteSpace(rankingText))
            rankingText = "Ainda não há usuários no ranking deste servidor.";

        int userPosition = await _mongo.GuildUserService.GetChatPosition(guild.Id, message.Author.Id);
        var authorUser = await _mongo.GuildUserService.GetUser(guild.Id, message.Author.Id);

        string userXpText = authorUser != null ? $"{authorUser.ChatXp:N0} XP" : "0 XP";
        string positionText = userPosition > 0 ? $"`#{userPosition}`" : "`-`";

        embed.Description = rankingText;
        embed.AddField("Sua posição", $"{positionText} — {message.Author.Username} (`{userXpText}`)");
        embed.WithFooter("Página 1");

        var buttons = new ComponentBuilder()
            .WithButton("◀️", "top_prev_chat_1", ButtonStyle.Primary)
            .WithButton("▶️", "top_next_chat_1", ButtonStyle.Primary);

        await message.Channel.SendMessageAsync(
            embed: embed.Build(),
            components: buttons.Build()
        );
    }

    public async Task ExecuteTopTxtAsync(SocketMessage message, SocketGuild guild)
    {
        await ExecuteTopAsync(message, guild);
    }

    public async Task ExecuteTopVozAsync(SocketMessage message, SocketGuild guild)
    {
        int page = 1;
        int pageSize = 10;

        var topUsers = await _mongo.GuildUserService.GetTopCall(guild.Id, page, pageSize);

        if (topUsers == null || topUsers.Count == 0)
        {
            await message.Channel.SendMessageAsync("Ainda não há usuários no ranking de call deste servidor.");
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle("🏆 Ranking de Call XP")
            .WithColor(Color.Gold);

        var ranking = new StringBuilder();
        int position = 1;

        foreach (var user in topUsers)
        {
            string name = guild.GetUser(user.UserId)?.DisplayName ?? user.UserName;

            string medal = position switch
            {
                1 => "🥇",
                2 => "🥈",
                3 => "🥉",
                _ => $"`#{position}`"
            };

            ranking.AppendLine($"{medal} **{name}** — `{user.CallXp:N0} XP`");
            position++;
        }

        string rankingText = ranking.ToString();
        if (string.IsNullOrWhiteSpace(rankingText))
            rankingText = "Ainda não há usuários no ranking de call deste servidor.";

        int userPosition = await _mongo.GuildUserService.GetCallPosition(guild.Id, message.Author.Id);
        var authorUser = await _mongo.GuildUserService.GetUser(guild.Id, message.Author.Id);

        string userXpText = authorUser != null ? $"{authorUser.CallXp:N0} XP" : "0 XP";
        string positionText = userPosition > 0 ? $"`#{userPosition}`" : "`-`";

        embed.Description = rankingText;
        embed.AddField("Sua posição", $"{positionText} — {message.Author.Username} (`{userXpText}`)");
        embed.WithFooter("Página 1");

        var buttons = new ComponentBuilder()
            .WithButton("◀️", "top_prev_call_1", ButtonStyle.Primary)
            .WithButton("▶️", "top_next_call_1", ButtonStyle.Primary);

        await message.Channel.SendMessageAsync(
            embed: embed.Build(),
            components: buttons.Build()
        );
    }
}