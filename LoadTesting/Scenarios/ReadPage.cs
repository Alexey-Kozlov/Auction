using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Data;
using NBomber.Data.CSharp;
using NBomber.Http.CSharp;
using Scenarios.DTO;

namespace Scenarios;

public class ReadPage : BasicScenario, IScenario
{
    public ReadPage(string test_system) : base(test_system) { }

    public ScenarioProps Run()
    {
        //var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        //
        var projectDirectory = Environment.CurrentDirectory;
        var urls = Data.LoadJson<PageUrl[]>($"{projectDirectory}/Configs/{Test_system}/ReadPageUrl.json");
        var loginData = Data.LoadJson<Login>($"{projectDirectory}/Configs/{Test_system}/login.json");
        var rndUrl = DataFeed.Circular(urls);
        var httpClient = Http.CreateDefaultClient();
        return Scenario.Create("readpage_scenario", async context =>
        {
            var login = Http.CreateRequest("POST", loginData.url)
                .WithHeader("Content-Type", "application/json")
                .WithJsonBody(loginData);

            var responseLogin = await Http.Send<ApiResponse<LoginResponseDTO>>(httpClient, login);

            var request = Http.CreateRequest("GET", rndUrl.GetNextItem(context.ScenarioInfo).Url)
                                .WithHeader("Accept", "application/json")
                                .WithHeader("Authorization", $"Bearer {responseLogin.Payload.Value.Data.Result.Token}");

            var response = await Http.Send(httpClient, request);

            return responseLogin;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.RampingConstant(copies: 1,
                                     during: TimeSpan.FromSeconds(1)),

            Simulation.RampingConstant(copies: 0,
                                     during: TimeSpan.FromSeconds(1))
        );
    }

}