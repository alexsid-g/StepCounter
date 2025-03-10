public class TeamStepCounterService
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, int>> _teams = new();

    public Task<bool> AddTeam(Guid teamId)
    {
        return Task.FromResult(_teams.TryAdd(teamId, new ConcurrentDictionary<Guid, int>()));
    }

    public Task DeleteTeam(Guid teamId)
    {
        _teams.TryRemove(teamId, out _);
        return Task.CompletedTask;
    }

    public Task<bool> AddCounter(Guid teamId, Guid counterId)
    {
        if (_teams.TryGetValue(teamId, out var team))
        {
            return Task.FromResult(team.TryAdd(counterId, 0));
        }
        return Task.FromResult(false);
    }

    public Task DeleteCounter(Guid teamId, Guid counterId)
    {
        if (_teams.TryGetValue(teamId, out var team))
        {
            team.TryRemove(counterId, out _);
        }
        return Task.CompletedTask;
    }

    public bool IncrementCounter(Guid teamId, Guid counterId, int steps)
    {
        if (_teams.TryGetValue(teamId, out var team) && team.ContainsKey(counterId))
        {
            team.AddOrUpdate(counterId, 0, (key, oldValue) => oldValue + steps);
            return true;
        }
        return false;
    }

    public int? GetTotalSteps(Guid teamId)
    {
        return _teams.TryGetValue(teamId, out var team) ? team.Values.Sum() : (int?)null;
    }

    public Dictionary<Guid, int>? ListCounters(Guid teamId)
    {
        return _teams.TryGetValue(teamId, out var team) ? team.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : null;
    }

    public Dictionary<Guid, int> ListTeams()
    {
        return _teams.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Values.Sum());
    }
}