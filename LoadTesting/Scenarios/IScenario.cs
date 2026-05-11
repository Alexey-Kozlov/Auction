using NBomber.Contracts;

namespace Scenarios;

public interface IScenario
{
    public ScenarioProps Run();
}