public class DiceService
{
    private readonly Random _rand = new();

    // método para rolar dados simples
    public (List<int> rolls, int total) Roll(int quantidade, int lados)
    {
        List<int> resultados = new();
        int total = 0;

        // rola os dados e acumula os resultados
        for (int i = 0; i < quantidade; i++)
        {
            int roll = _rand.Next(1, lados + 1);
            resultados.Add(roll);
            total += roll;
        }

        return (resultados, total);
    }

    // método para rolar dados com explosão
    public (List<int> rolls, int total) RollExploding(int quantidade, int lados, int? explodeThreshold = null)
    {
        if (quantidade <= 0 || lados <= 0)
            throw new Exception("Dados com valores inválidos.");

        // define o limite de explosão, usando o número de lados se não for especificado
        int limite = explodeThreshold ?? lados;

        if (limite <= 1)
            throw new Exception("O valor após '!' deve ser maior que 1.");

        if (limite > lados)
            throw new Exception("O valor após '!' não pode ser maior que o número de lados do dado.");

        // lista para armazenar os resultados e o total
        List<int> resultados = new();
        int total = 0;

        // trava de segurança
        int maxRolls = 1000;

        // rola os dados iniciais
        for (int i = 0; i < quantidade; i++)
        {
            int atual = _rand.Next(1, lados + 1);
            resultados.Add(atual);
            total += atual;

            // continua rolando enquanto o resultado for maior ou igual ao limite
            while (atual >= limite)
            {
                if (resultados.Count >= maxRolls)
                    throw new Exception("Limite de explosões atingido.");

                atual = _rand.Next(1, lados + 1);
                resultados.Add(atual);
                total += atual;
            }
        }

        // retorna a lista de resultados e o total
        return (resultados, total);
    }
}