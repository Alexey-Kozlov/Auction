using NBomber.Contracts;
using NBomber.CSharp;

namespace Scenarios;

public class EditPage : BasicScenario, IScenario
{
    public EditPage(string test_system) : base(test_system) { }

    public ScenarioProps Run()
    {
        var scenario = Scenario.Create("data_example", async context =>
        {
            var step1 = await Step.Run("create_user", context, async () =>
            {
                var userId = "user_123";
                context.Data["UserId"] = userId;
                return Response.Ok(payload: userId);
            });

            var step2 = await Step.Run("get_user", context, async () =>
            {
                var userId = (string)context.Data["UserId"];
                // use userId from previous step
                return Response.Ok();
            });

            return Response.Ok();
        });
        return scenario;
    }

}