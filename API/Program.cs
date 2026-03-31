using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("=== BUILD NOVO 31/03 LOGIN TESTE V2 ===");
Console.WriteLine("ASPNETCORE_URLS raw: " + Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
Console.WriteLine("PORT raw: " + Environment.GetEnvironmentVariable("PORT"));
Console.WriteLine("ASPNETCORE_ENVIRONMENT raw: " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

builder.Configuration
    .AddJsonFile("Settings/appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile(
        $"Settings/appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true)
    .AddEnvironmentVariables();

Console.WriteLine("ClientId raw: " + builder.Configuration["DiscordOAuth:ClientId"]);

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
            .WithOrigins("https://joseboylovemaker.github.io")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};

forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

var app = builder.Build();

app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = "Erro interno na API",
            path = feature?.Path,
            detail = feature?.Error?.Message
        });
    });
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Painel");
app.UseAuthorization();

app.MapGet("/", () => "BUILD NOVO V2");
app.MapGet("/health", () => Results.Ok(new
{
    ok = true,
    env = app.Environment.EnvironmentName,
    time = DateTime.UtcNow
}));

app.MapControllers();

app.Run();