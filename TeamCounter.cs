public class TeamCounter
{
    // Property for the identifier or key
    public string Counter { get; set; }

    // Property for the steps or measurement
    public int Steps { get; set; }

    // Constructor to initialize the properties
    public TeamCounter(string counter, int steps)
    {
        Counter = counter;
        Steps = steps;
    }
}