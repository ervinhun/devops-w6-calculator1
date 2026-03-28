using Calculator;
using Calculator.Api.Persistence;
using Calculator.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__CalculatorDb")
    ?? "Host=localhost;Port=5432;Database=calculator;Username=calculator_app;Password=calculator_app";

builder.Services.AddControllers();
builder.Services.AddScoped<ICalculator, CachedCalculator>();
builder.Services.AddSingleton<ICalculationHistoryRepository>(_ => new CalculationHistoryRepository(connectionString));
builder.Services.AddSingleton<ICalculationService, CalculationService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();
app.MapControllers();

await app.RunAsync();

public partial class Program;
