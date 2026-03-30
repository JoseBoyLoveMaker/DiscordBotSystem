using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

public class ExpressionDiceService
{
    // serviço para avaliar expressões de dados complexas
    private readonly DiceService _dice = new();
    private readonly Random _rand = new();

    // método principal para avaliar expressões de dados
    public string Evaluate(string input)
    {
        // valida entrada vazia
        if (string.IsNullOrWhiteSpace(input))
            throw new Exception("Expressão vazia.");

        // normaliza a expressão para facilitar o processamento
        string normalized = NormalizeExpression(input);

        // se a expressão contém '#', trata como expressão repetida, caso contrário, avalia normalmente
        if (normalized.Contains('#'))
            return EvaluateRepeatedExpression(normalized);

        // avalia expressão única
        return EvaluateSingleExpression(normalized);
    }

    // método para avaliar expressões que usam o modificador de repetição '#'
    private string EvaluateRepeatedExpression(string expression)
    {
        // extrai o número de repetições e a expressão interna usando regex
        var match = Regex.Match(expression, @"^(\d+)#(.+)$");

        if (!match.Success)
            throw new Exception("Formato inválido para '#'. Use algo como `2#d20+3`.");

        int repetitions = int.Parse(match.Groups[1].Value);
        string innerExpression = match.Groups[2].Value;
        
        if (repetitions <= 0)
            throw new Exception("A quantidade de repetições deve ser maior que 0.");

        if (repetitions > 50)
            throw new Exception("Quantidade de repetições muito alta.");

        List<string> lines = new();

        for (int i = 0; i < repetitions; i++)
            lines.Add(EvaluateSingleExpression(innerExpression));

        return string.Join("\n", lines);
    }

    // método para avaliar expressões sem o modificador de repetição
    private string EvaluateSingleExpression(string expression)
    {
        ValidateSingleExpression(expression);
        ValidateShuffleModifier(expression);

        var diceMatches = Regex.Matches(expression, @"(\d+)d(\d+)(?:!(\d*)?)?(a?)");
        var replacements = new List<(string token, string shown, string value)>();

        // processa cada ocorrência de dados na expressão
        foreach (Match match in diceMatches)
        {
            int quantidade = int.Parse(match.Groups[1].Value);
            int lados = int.Parse(match.Groups[2].Value);

            bool exploding = match.Value.Contains('!');
            string thresholdText = match.Groups[3].Value;

            bool shuffle = match.Groups[4].Value == "a";

            if (quantidade <= 0 || lados <= 0)
                throw new Exception("Dados com valores inválidos.");

            if (quantidade > 100)
                throw new Exception("Quantidade de dados muito alta.");

            List<int> rolls;
            int diceTotal;

            // rola os dados, usando o método de explosão se necessário
            if (exploding)
            {
                int? explodeThreshold = null;

                if (!string.IsNullOrWhiteSpace(thresholdText))
                {
                    explodeThreshold = int.Parse(thresholdText);

                    if (explodeThreshold <= 1)
                        throw new Exception("O valor após '!' deve ser maior que 1.");
                }

                var diceResult = _dice.RollExploding(quantidade, lados, explodeThreshold);
                rolls = diceResult.rolls;
                diceTotal = diceResult.total;
            }
            else
            {
                var diceResult = _dice.Roll(quantidade, lados);
                rolls = diceResult.rolls;
                diceTotal = diceResult.total;
            }

            // ordena os resultados, embaralhando se o modificador 'a' estiver presente
            List<int> finalRolls;

            if (shuffle)
            {
                finalRolls = ShuffleRolls(rolls);
            }
            else
            {
                finalRolls = rolls
                    .OrderByDescending(x => x)
                    .ToList();
            }

            // formata os resultados para exibição
            List<string> displayRolls = finalRolls
            .Select(x => x == lados ? $"**{x}**" : x.ToString())
            .ToList();

            string shown = quantidade == 1 && displayRolls.Count == 1
                ? $"[{displayRolls[0]}]"
                : $"[{string.Join(", ", displayRolls)}]";

            replacements.Add((match.Value, shown, diceTotal.ToString(CultureInfo.InvariantCulture)));
        }

        // substitui as ocorrências de dados na expressão original pelos resultados para exibição e para cálculo
        string shownExpression = expression;
        string mathExpression = expression;

        // as substituições são feitas apenas na primeira ocorrência para evitar problemas com expressões como "2d6 + 2d6"
        foreach (var item in replacements)
        {
            shownExpression = ReplaceFirst(shownExpression, item.token, item.shown);
            mathExpression = ReplaceFirst(mathExpression, item.token, item.value);
        }

        // avalia a expressão matemática resultante
        double total;
        if (mathExpression.Contains("a"))
            throw new Exception("Uso inválido do modificador 'a'.");
        try
        {
            total = EvaluateAdvancedMath(mathExpression);
        }
        catch
        {
            throw new Exception("Expressão matemática inválida.");
        }

        // formata o resultado final para exibição
        string totalText = FormatNumber(total);
        string finalShown = BuildCompactShown(expression, shownExpression);

        return $"` {totalText} ` ← {finalShown}";
    }

    // método para avaliar expressões matemáticas avançadas, incluindo suporte para parênteses, potência, raiz e porcentagem
    private double EvaluateAdvancedMath(string expression)
    {
        string expr = expression.Replace(" ", "");

        // resolve expressões dentro de parênteses primeiro, usando recursão
        while (expr.Contains("("))
        {
            // regex para encontrar a expressão mais interna entre parênteses
            expr = Regex.Replace(expr, @"\(([^()]+)\)", match =>
            {
                double inner = EvaluateFlatExpression(match.Groups[1].Value);
                return inner.ToString(CultureInfo.InvariantCulture);
            });
        }

        // avalia a expressão final sem parênteses
        return EvaluateFlatExpression(expr);
    }

    private double EvaluateFlatExpression(string expression)
    {
        string expr = expression;

        // potência
        expr = ResolveOperator(expr, @"(-?\d+(?:\.\d+)?)\^(-?\d+(?:\.\d+)?)", (a, b) =>
            Math.Pow(a, b));

        // raiz: 9z2 = raiz quadrada de 9
        expr = ResolveOperator(expr, @"(-?\d+(?:\.\d+)?)z(-?\d+(?:\.\d+)?)", (a, b) =>
        {
            if (b == 0)
                throw new Exception("Raiz com índice 0 é inválida.");

            if (a < 0 && b % 2 == 0)
                throw new Exception("Raiz par de número negativo é inválida.");

            return Math.Pow(a, 1.0 / b);
        });

        // porcentagem: 50%200 = 50% de 200
        expr = ResolveOperator(expr, @"(-?\d+(?:\.\d+)?)%(-?\d+(?:\.\d+)?)", (a, b) =>
            (a / 100.0) * b);

        object mathResult = new DataTable().Compute(expr, null);
        return Convert.ToDouble(mathResult, CultureInfo.InvariantCulture);
    }

    // método auxiliar para resolver operações binárias usando regex, aplicando a operação fornecida
    private string ResolveOperator(string expression, string pattern, Func<double, double, double> operation)
    {
        string expr = expression;

        while (Regex.IsMatch(expr, pattern))
        {
            var regex = new Regex(pattern);

            expr = regex.Replace(expr, match =>
            {
                double a = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                double b = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);

                double result = operation(a, b);

                return result.ToString(CultureInfo.InvariantCulture);
            }, 1);
        }

        return expr;
    }

    // método para construir a parte da expressão mostrada ao usuário, mantendo os resultados dos dados e a estrutura original
    private string BuildCompactShown(string originalExpression, string shownExpression)
    {
        if (originalExpression == shownExpression)
            return AddSpaces(originalExpression);

        var firstDiceMatch = Regex.Match(originalExpression, @"(\d+)d(\d+)(?:!(\d*)?)?(a?)");
        if (!firstDiceMatch.Success)
            return AddSpaces(shownExpression);

        var shownDiceMatch = Regex.Match(shownExpression, @"\[[^\]]+\]");
        if (!shownDiceMatch.Success)
            return AddSpaces(shownExpression);

        string originalWithSpaces = AddSpaces(originalExpression);
        return $"{shownDiceMatch.Value} {originalWithSpaces}";
    }

    // método para normalizar a expressão de entrada, facilitando o processamento posterior
    private string NormalizeExpression(string input)
    {
        string value = input.ToLower().Trim();
        value = value.Replace(" ", "");

        // d20 -> 1d20
        value = Regex.Replace(value, @"(?<!\d)d(\d+)", "1d$1");

        return value;
    }

    // método para validar expressões sem o modificador de repetição, verificando tamanho, caracteres permitidos e balanceamento de parênteses
    private void ValidateSingleExpression(string expression)
    {
        if (expression.Length > 120)
            throw new Exception("Expressão muito grande.");

        if (!Regex.IsMatch(expression, @"^[0-9d+\-*/().!^z%a]+$"))
            throw new Exception("A expressão contém caracteres inválidos.");

        int balance = 0;
        foreach (char c in expression)
        {
            if (c == '(') balance++;
            if (c == ')') balance--;

            if (balance < 0)
                throw new Exception("Parênteses inválidos.");
        }

        if (balance != 0)
            throw new Exception("Parênteses inválidos.");
    }

    // método auxiliar para substituir apenas a primeira ocorrência de uma substring, usado para construir a expressão mostrada ao usuário
    private string ReplaceFirst(string text, string search, string replacement)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
            return text;

        return text.Substring(0, pos) + replacement + text.Substring(pos + search.Length);
    }

    // método para formatar números, removendo casas decimais desnecessárias para inteiros
    private string FormatNumber(double value)
    {
        return value % 1 == 0
            ? ((int)value).ToString()
            : value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    //  método para adicionar espaços em torno de operadores na expressão mostrada ao usuário, melhorando a legibilidade
    private string AddSpaces(string expression)
    {
        return expression
            .Replace("+", " + ")
            .Replace("-", " - ")
            .Replace("*", " * ")
            .Replace("/", " / ")
            .Replace("^", " ^ ")
            .Replace("z", " z ")
            .Replace("%", " % ")
            .Replace("(", "( ")
            .Replace(")", " )")
            .Replace("  ", " ")
            .Trim();
    }

    // método para validar o uso do modificador 'a', garantindo que ele esteja colado ao dado e não seja usado de forma inválida
    private void ValidateShuffleModifier(string expression)
    {
        var invalidA = Regex.Match(expression, @"(^|[^0-9d])a");

        // remove casos válidos tipo "4d20a"
        var validMatches = Regex.Matches(expression, @"\d+d\d+(?:!\d*)?a");

        foreach (Match valid in validMatches)
        {
            expression = expression.Replace(valid.Value, "");
        }

        if (expression.Contains("a"))
            throw new Exception("O modificador 'a' deve ficar colado ao dado. Ex: `4d55a`.");
    }

    // método para avaliar expressões que contêm apenas operações matemáticas, sem dados, usado para validar expressões repetidas
    public string EvaluateMathOnly(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new Exception("Expressão vazia.");

        string expression = input.ToLower().Trim().Replace(" ", "");

        if (!Regex.IsMatch(expression, @"^[0-9+\-*/().^z%]+$"))
            throw new Exception("A expressão contém caracteres inválidos.");

        int balance = 0;
        foreach (char c in expression)
        {
            if (c == '(') balance++;
            if (c == ')') balance--;

            if (balance < 0)
                throw new Exception("Parênteses inválidos.");
        }

        if (balance != 0)
            throw new Exception("Parênteses inválidos.");

        double total;
        try
        {
            total = EvaluateAdvancedMath(expression);
        }
        catch
        {
            throw new Exception("Expressão matemática inválida.");
        }

        string totalText = FormatNumber(total);
        string shown = AddSpaces(expression);

        return $"`{totalText}` ← {shown}";
    }

    // método para embaralhar os resultados dos dados quando o modificador 'a' é usado, garantindo que a ordem seja aleatória
    private List<int> ShuffleRolls(List<int> rolls)
    {
        var result = new List<int>(rolls);

        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = _rand.Next(i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }

        return result;
    }
}