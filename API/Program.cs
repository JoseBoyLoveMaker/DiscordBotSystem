var builder = WebApplication.CreateBuilder(args);

// Limpa as fontes padrão e carrega os appsettings da pasta Settings
builder.Configuration.Sources.Clear();

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("Settings/appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile(
        $"Settings/appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mongoSettings = builder.Configuration
    .GetSection("MongoSettings")
    .Get<MongoSettings>();

if (mongoSettings == null)
{
    throw new Exception("MongoSettings não foi encontrado no appsettings.json da API.");
}

var discordOAuthSettings = builder.Configuration
    .GetSection("DiscordOAuth")
    .Get<DiscordOAuthSettings>();

if (discordOAuthSettings == null)
{
    throw new Exception("DiscordOAuth não foi encontrado no appsettings.json da API.");
}

builder.Services.AddSingleton(mongoSettings);
builder.Services.AddSingleton(discordOAuthSettings);

builder.Services.AddSingleton<MongoHandlerAPI>();

builder.Services.AddScoped<UserServiceAPI>();
builder.Services.AddScoped<ResponseServiceAPI>();
builder.Services.AddScoped<BotStatusServiceAPI>();
builder.Services.AddScoped<CommandServiceAPI>();

builder.Services.AddSingleton<UserSessionStore>();
builder.Services.AddHttpClient<DiscordAuthService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Painel", policy =>
    {
        policy
            .WithOrigins(
                "https://joseboylovemaker.github.io/DiscordBotSystem/"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();



app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseCors("Painel");
app.UseAuthorization();
app.MapControllers();

app.Run();