#pragma warning disable EXTEXP0018 

using Microsoft.Extensions.Caching.Hybrid;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHybridCache();

var configurationOptions = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("cache") ?? throw new InvalidOperationException("Could not find a 'redisvss' connection string."));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.ConfigurationOptions = configurationOptions;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

HybridCache hybridCache = app.Services.GetRequiredService<HybridCache>();

app.MapGet("/weatherforecast", async () =>
{
    var entryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromHours(1),
        LocalCacheExpiration = TimeSpan.FromHours(1)
    };

    var forecast = await hybridCache.GetOrCreateAsync($"weatherforecast-{DateTime.Today} ", 
                                    async _ => await Task.FromResult(Enumerable.Range(1, 5).Select(index =>
                                    new WeatherForecast
                                    (
                                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                        Random.Shared.Next(-20, 55),
                                        summaries[Random.Shared.Next(summaries.Length)]
                                    ))
                                    .ToArray()),
                                    entryOptions);

    return forecast;
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
