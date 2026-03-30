using Discord;
using Discord.WebSocket;

public class RollCommand
{
    // serviço para avaliar expressões de dados
    private readonly ExpressionDiceService _expressionDice = new();

    public async Task ExecuteAsync(SocketMessage message)
    {
        // ignora mensagens que não começam com "q"
        string input = message.Content.Trim();

        if (!input.StartsWith("q"))
            return;

        string expression = input.Substring(1).Trim();

        // avalia a expressão e envia o resultado
        try
        {
            string outputText = _expressionDice.Evaluate(expression);

            await message.Channel.SendMessageAsync(
                outputText,
                messageReference: new MessageReference(message.Id)
            );
        }

        // mostra erro de expressão inválida
        catch (Exception ex)
        {
            await message.Channel.SendMessageAsync(
                $"❌ {ex.Message}",
                messageReference: new MessageReference(message.Id)
            );
        }
    }
}