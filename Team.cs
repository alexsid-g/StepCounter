public class Team
{
    // Property for the identifier or key
    public string Team { get; set; }

    // Property for the steps or measurement
    public int TotalSteps { get; set; }

    // Constructor to initialize the properties
    public Team(string team, int totalSteps)
    {
        Team = team;
        TotalSteps = totalSteps;
    }
}