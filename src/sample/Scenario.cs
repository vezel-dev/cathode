namespace Sample;

abstract class Scenario
{
    public string Name => GetType().Name[..^"Scenario".Length];

    public abstract Task RunAsync();
}
