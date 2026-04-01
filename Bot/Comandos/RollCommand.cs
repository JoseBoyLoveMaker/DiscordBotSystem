using Discord;
using Discord.WebSocket;

public class RollCommand
{
    // serviço para avaliar expressões de dados
    private readonly ExpressionDiceService _expressionDice = new();

    public async Task ExecuteAsync(SocketMessage message)
    {
        string content = message.Content.Trim();
        var parts = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            await message.Channel.SendMessageAsync(
                "❌ Use o comando corretamente. Ex: `!roll 1d20+5`",
                messageReference: new MessageReference(message.Id)
            );
            return;
        }

        // pega tudo depois do nome/alias do comando
        string expression = content.Substring(parts[0].Length).Trim();

        if (string.IsNullOrWhiteSpace(expression))
        {
            await message.Channel.SendMessageAsync(
                "❌ Escreva uma expressão de dados. Ex: `!roll 1d20+5`",
                messageReference: new MessageReference(message.Id)
            );
            return;
        }

        try
        {
            string outputText = _expressionDice.Evaluate(expression);

            await message.Channel.SendMessageAsync(
                outputText,
                messageReference: new MessageReference(message.Id)
            );
        }
        catch (Exception ex)
        {
            await message.Channel.SendMessageAsync(
                $"❌ {ex.Message}",
                messageReference: new MessageReference(message.Id)
            );
        }
    }
}