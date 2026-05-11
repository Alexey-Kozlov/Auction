using NBomber.CSharp;
using Scenarios;

internal class Program
{
    private static void Main(string[] args)
    {
        var test_system = args.Length == 0 ? "Kuber_json" : "Dev_json";
        //Console.WriteLine(test_system);
        NBomberRunner.RegisterScenarios(
        //new MainPage(test_system).Run(),
        new ReadPage(test_system).Run())
        .Run();
    }
}