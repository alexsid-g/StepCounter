using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class TeamStepCounterService
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _teams = new();

    public Task<bool> AddTeam(string teamId)
    {
        return Task.FromResult(_teams.TryAdd(teamId, new ConcurrentDictionary<string, int>()));
    }

    public Task DeleteTeam(string teamId)
    {
        _teams.TryRemove(teamId, out _);
        return Task.CompletedTask;
    }

    public Task<bool> AddCounter(string teamId, string counterId)
    {
        if (_teams.TryGetValue(teamId, out var team))
        {
            return Task.FromResult(team.TryAdd(counterId, 0));
        }
        return Task.FromResult(false);
    }

    public Task DeleteCounter(string teamId, string counterId)
    {
        if (_teams.TryGetValue(teamId, out var team))
        {
            team.TryRemove(counterId, out _);
        }
        return Task.CompletedTask;
    }

    public bool IncrementCounter(string teamId, string counterId, int steps)
    {
        if (_teams.TryGetValue(teamId, out var team) && team.ContainsKey(counterId))
        {
            team.AddOrUpdate(counterId, 0, (key, oldValue) => oldValue + steps);
            return true;
        }
        return false;
    }

    public int? GetTotalSteps(string teamId)
    {
        return _teams.TryGetValue(teamId, out var team) ? team.Values.Sum() : (int?)null;
    }

    public List<object>? ListCounters(string teamId)
    {
        return _teams.TryGetValue(teamId, out var team) 
            ? team.Select(x => new { Counter = x.Key, Steps = x.Value}).ToList()
            : null;
    }

    public List<object> ListTeams()
    {
        return _teams.Select(x => new { Team = x.Key, TotalSteps = x.Value.Values.Sum()}).ToList();
    }
}