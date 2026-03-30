using Discord;
using Discord.WebSocket;

public class EldenCommand
{
    private readonly MongoHandler _mongo;

    public EldenCommand(MongoHandler mongo)
    {
        _mongo = mongo;
    }

    public async Task ExecuteAsync(SocketMessage message)
    {
        var embed = new EmbedBuilder()
            .WithTitle("⚔️ Elden Ring ⚔️")
            .WithDescription("Selecione uma categoria para visualizar o catálogo.")
            .WithColor(Color.DarkPurple)
            .Build();

        var buttons = new ComponentBuilder()
            .WithButton("Armas", "elden_menu_category_Armas", ButtonStyle.Primary)
            .WithButton("Armaduras", "elden_menu_category_Armaduras", ButtonStyle.Secondary)
            .WithButton("Talismãs", "elden_menu_category_Talismas", ButtonStyle.Secondary)
            .WithButton("Cinzas de Guerra", "elden_menu_category_Cinzas", ButtonStyle.Secondary)
            .WithButton("Feitiços", "elden_menu_category_Feiticos", ButtonStyle.Secondary);

        await message.Channel.SendMessageAsync(
            embed: embed,
            components: buttons.Build()
        );
    }
}