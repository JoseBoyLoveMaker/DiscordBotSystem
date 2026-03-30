using Discord;
using Discord.WebSocket;

public class MathCommand
{
    private readonly ExpressionDiceService _expressionDice = new();

    // Método para lidar com mensagens que começam com "m" e avaliar expressões matemáticas
    public async Task ExecuteAsync(SocketMessage message)
    {
        string input = message.Content.Trim();

        if (!input.StartsWith("m"))
            return;

        string expression = input.Substring(1).Trim();

        if (string.IsNullOrWhiteSpace(expression))
        {
            await message.Channel.SendMessageAsync(
                "❌ Escreva uma expressão. Ex: `m2+3*4`",
                messageReference: new MessageReference(message.Id)
            );
            return;
        }

        // Verifica se a expressão contém caracteres de dados, o que não é permitido
        if (expression.Contains('d') || expression.Contains('#') || expression.Contains('!'))
        {
            await message.Channel.SendMessageAsync(
                "❌ O comando `m` aceita apenas matemática, sem dados.",
                messageReference: new MessageReference(message.Id)
            );
            return;
        }

        // Avalia a expressão matemática e envia o resultado
        try
        {
            string result = _expressionDice.EvaluateMathOnly(expression);

            await message.Channel.SendMessageAsync(
                result,
                messageReference: new MessageReference(message.Id)
            );
        }
        // Mostra erro de expressão inválida
        catch (Exception ex)
        {
            await message.Channel.SendMessageAsync(
                $"❌ {ex.Message}",
                messageReference: new MessageReference(message.Id)
            );
        }
    }
}