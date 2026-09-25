using ImageCards.Api.Configuration;
using ImageCards.Api.Data;
using ImageCards.Api.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddImageCardsApi()
    .AddImageCardsDatabase()
    .AddImageCardsStorage(builder.Configuration)
    .AddImageCardsCurrentUser(builder.Configuration, builder.Environment)
    .AddImageCardsServices()
    .AddImageCardsHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.MapControllers();
app.MapImageCardsHealthChecks();

await app.InitializeDatabaseAsync();
await app.RunAsync();

/// <summary>Exposed for WebApplicationFactory in the integration tests.</summary>
public partial class Program;
