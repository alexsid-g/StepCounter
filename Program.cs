using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSwag;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "TeamStepCounter API";
    config.Version = "v1";
    config.Description = "This is API using NSwag for TeamStepCounter documentation.";
});
builder.Services.AddSingleton<TeamStepCounterService>();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Swagger
app.UseOpenApi();
app.UseSwaggerUi();

// Health Checks
app.MapHealthChecks("/healthz");

// Dependency Injection
var service = app.Services.GetRequiredService<TeamStepCounterService>();

app.MapPost("/teams/{teamId}/counters/{counterId}", async (string teamId, string counterId) =>
{
    var result = await service.AddCounter(teamId, counterId);
    return result ? Results.Ok($"Counter {counterId} added to team {teamId}.") : Results.BadRequest("Failed to add counter.");
});

// Increment a counter
app.MapPost("/teams/{teamId}/counters/{counterId}/increment", async (string teamId, string counterId, [FromBody] int steps) =>
{
    if (service.IncrementCounter(teamId, counterId, steps))
    {
        return Results.Ok($"{steps} steps added to {counterId} in {teamId}.");
    }
    return Results.NotFound("Counter not found.");
});

// Get total steps for a team
app.MapGet("/teams/{teamId}/total", async (string teamId) =>
{
    var totalSteps = service.GetTotalSteps(teamId);
    return totalSteps.HasValue ? Results.Ok(totalSteps.Value) : Results.NotFound("Team not found.");
});

// List all teams and their total steps
app.MapGet("/teams", async () => Results.Ok(service.ListTeams()));

// List all counters in a team
app.MapGet("/teams/{teamId}/counters", async (string teamId) =>
{
    var counters = service.ListCounters(teamId);
    return counters != null ? Results.Ok(counters) : Results.NotFound("Team not found.");
});

// Add/Delete teams
app.MapPost("/teams/{teamId}", async (string teamId) =>
{
    var result = await service.AddTeam(teamId);
    return result ? Results.Ok($"Team {teamId} added.") : Results.BadRequest("Failed to add team.");
});

app.MapDelete("/teams/{teamId}", async (string teamId) =>
{
    await service.DeleteTeam(teamId);
    return Results.Ok($"Team {teamId} deleted.");
});

// Add/Delete counters
app.MapDelete("/teams/{teamId}/counters/{counterId}", async (string teamId, string counterId) =>
{
    await service.DeleteCounter(teamId, counterId);
    return Results.Ok($"Counter {counterId} deleted from team {teamId}.");
});

app.Run();
