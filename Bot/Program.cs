using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("Settings/appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("Settings/appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var bot = new Bot(configuration);
        await bot.StartAsync();
    }
}