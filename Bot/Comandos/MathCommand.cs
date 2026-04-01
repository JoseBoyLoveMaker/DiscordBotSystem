using Discord;
using Discord.WebSocket;

public class MathCommand
{
    private readonly ExpressionDiceService _expressionDice = new();

    public async Task ExecuteAsync(SocketMessage message)
    {
        string content = message.Content.Trim();
        var parts = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            await message.Channel.SendMessageAsync(
                "❌ Escreva uma expressão. Ex: `!math 2+3*4`",
                messageReference: new MessageReference(message.Id)
            );
            return;
        }

        // pega tudo depois do nome/alias do comando
        string expression = content.Substring(parts[0].Length).Trim();

        if (string.IsNullOrWhiteSpace(expression))
        {
            await message.Channel.SendMessageAsync(
                "❌ Escreva uma expressão. Ex: `!math 2+3*4`",
                messageReference: new MessageReference(message.Id)
            );
            return;
        }

        // matemática pura: sem dados
        if (expression.Contains('d') || expression.Contains('D') ||
            expression.Contains('#') || expression.Contains('!') ||
            expression.Contains('a') || expression.Contains('A'))
        {
            await message.Channel.SendMessageAsync(
                "❌ O comando de matemática aceita apenas matemática, sem dados.",
                messageReference: new MessageReference(message.Id)
            );
            return;
        }

        try
        {
            string result = _expressionDice.EvaluateMathOnly(expression);

            await message.Channel.SendMessageAsync(
                result,
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