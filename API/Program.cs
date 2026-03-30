var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddSingleton(mongoSettings);
builder.Services.AddSingleton<MongoHandlerAPI>();

builder.Services.AddScoped<UserServiceAPI>();
builder.Services.AddScoped<ResponseServiceAPI>();
builder.Services.AddScoped<BotStatusServiceAPI>();
builder.Services.AddScoped<CommandServiceAPI>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Painel", policy =>
    {
        policy
            .WithOrigins("https://localhost:7296")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Painel");
app.UseAuthorization();
app.MapControllers();

app.Run();