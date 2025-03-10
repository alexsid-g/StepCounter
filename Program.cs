using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NSwag.AspNetCore;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Step Counter API";
    config.Version = "v1";
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

// Create a new counter for a team
app.MapPost("/teams/{teamId}/counters/{counterId}", async (string teamId, string counterId) =>
{
    var result = await service.AddCounter(teamId, counterId);
    return result ? Results.Ok($"Counter {counterId} added to team {teamId}.") : Results.BadRequest("Failed to add counter.");
})
.WithName("CreateCounter")
.WithMetadata(new { 
    Summary = "Creates a new counter for a team",
    Description = "This endpoint adds a new counter to a specific team using the provided teamId and counterId.",
    ResponseType = typeof(string),
    ResponseStatusCode = 200, // OK when counter is added
    ResponseStatusCodeError = 400 // Bad Request if counter addition fails
});

// Increment a counter
app.MapPost("/teams/{teamId}/counters/{counterId}/increment", async (string teamId, string counterId, [FromBody] int steps) =>
{
    if (service.IncrementCounter(teamId, counterId, steps))
    {
        return Results.Ok($"{steps} steps added to {counterId} in {teamId}.");
    }
    return Results.NotFound("Counter not found.");
})
.WithName("IncrementCounter")
.WithMetadata(new { 
    Summary = "Increments steps for a specific counter in a team",
    Description = "This endpoint increments the step count for a counter in a given team by the provided number of steps.",
    ResponseType = typeof(string),
    ResponseStatusCode = 200, // OK when increment is successful
    ResponseStatusCodeError = 404 // Not Found if the counter does not exist
});

// Get total steps for a team
app.MapGet("/teams/{teamId}/total", async (string teamId) =>
{
    var totalSteps = service.GetTotalSteps(teamId);
    return totalSteps.HasValue ? Results.Ok(totalSteps.Value) : Results.NotFound("Team not found.");
})
.WithName("GetTeamTotalSteps")
.WithMetadata(new { 
    Summary = "Gets the total number of steps for a team",
    Description = "This endpoint retrieves the total number of steps across all counters for a specific team.",
    ResponseType = typeof(int),
    ResponseStatusCode = 200, // OK when team steps are found
    ResponseStatusCodeError = 404 // Not Found if the team does not exist
});

// List all teams and their total steps
app.MapGet("/teams", async () => Results.Ok(service.ListTeams()))
.WithName("ListTeams")
.WithMetadata(new { 
    Summary = "Lists all teams and their total steps",
    Description = "This endpoint returns a list of all teams and the total steps for each team.",
    ResponseType = typeof(List<TeamData>),
    ResponseStatusCode = 200
});

// List all counters in a team
app.MapGet("/teams/{teamId}/counters", async (string teamId) =>
{
    var counters = service.ListCounters(teamId);
    return counters != null ? Results.Ok(counters) : Results.NotFound("Team not found.");
})
.WithName("ListCounters")
.WithMetadata(new { 
    Summary = "Lists all counters in a team",
    Description = "This endpoint lists all counters belonging to a specific team.",
    ResponseType = typeof(List<TeamCounter>),
    ResponseStatusCode = 200, // OK when counters are found
    ResponseStatusCodeError = 404 // Not Found if the team does not exist
});

// Add/Delete teams
app.MapPost("/teams/{teamId}", async (string teamId) =>
{
    var result = await service.AddTeam(teamId);
    return result ? Results.Ok($"Team {teamId} added.") : Results.BadRequest("Failed to add team.");
})
.WithName("AddTeam")
.WithMetadata(new { 
    Summary = "Adds a new team",
    Description = "This endpoint adds a new team using the provided teamId.",
    ResponseType = typeof(string),
    ResponseStatusCode = 200, // OK when team is added
    ResponseStatusCodeError = 400 // Bad Request if the team already exists
});

app.MapDelete("/teams/{teamId}", async (string teamId) =>
{
    await service.DeleteTeam(teamId);
    return Results.Ok($"Team {teamId} deleted.");
})
.WithName("DeleteTeam")
.WithMetadata(new { 
    Summary = "Deletes a team",
    Description = "This endpoint deletes an existing team using the provided teamId.",
    ResponseType = typeof(string),
    ResponseStatusCode = 200 // OK when team is deleted
});

// Add/Delete counters
app.MapDelete("/teams/{teamId}/counters/{counterId}", async (string teamId, string counterId) =>
{
    await service.DeleteCounter(teamId, counterId);
    return Results.Ok($"Counter {counterId} deleted from team {teamId}.");
})
.WithName("DeleteCounter")
.WithMetadata(new { 
    Summary = "Deletes a counter from a team",
    Description = "This endpoint deletes a counter from a specified team using the provided teamId and counterId.",
    ResponseType = typeof(string),
    ResponseStatusCode = 200 // OK when counter is deleted
});

app.Run();
