using Microsoft.VisualBasic;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace Scenarios;

public class MainPage : BasicScenario, IScenario
{
    public MainPage(string test_system) : base(test_system) { }

    public ScenarioProps Run()
    {
        var httpClient = Http.CreateDefaultClient();

        return Scenario.Create("mainpageload_scenario", async context =>
        {
            var request = Http.CreateRequest("GET", "http://auction")
                                .WithHeader("Accept", "text/html");
            // .WithHeader("Accept", "application/json")
            // .WithBody(new StringContent("{ id: 1 }", Encoding.UTF8, "application/json");
            // .WithBody(new ByteArrayContent(new [] {1,2,3}))  

            var response = await Http.Send(httpClient, request);

            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.RampingInject(rate: 100,
                                     interval: TimeSpan.FromSeconds(1),
                                     during: TimeSpan.FromMinutes(1)),

            Simulation.Inject(rate: 100,
                              interval: TimeSpan.FromSeconds(1),
                              during: TimeSpan.FromSeconds(5))
        );

    }

}