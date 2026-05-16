using Common.Contracts;
using Common.Contracts.Auction;
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
        //Получение настроек
        var projectDirectory = Environment.CurrentDirectory;

        var auctions = Data.LoadJson<PageUrl[]>($"{projectDirectory}/Configs/{Test_system}/ReadPageUrl.json");
        var loginData = Data.LoadJson<Login>($"{projectDirectory}/Configs/{Test_system}/login.json");
        var identity = Data.LoadJson<Identity>($"{projectDirectory}/Configs/{Test_system}/identityUrl.json");
        var image = Data.LoadJson<Image>($"{projectDirectory}/Configs/{Test_system}/imageUrl.json");
        var currentPage = Data.LoadJson<SetCurrentPage>($"{projectDirectory}/Configs/{Test_system}/setCurrentPageUrl.json");
        var bids = Data.LoadJson<Bids>($"{projectDirectory}/Configs/{Test_system}/bidsUrl.json");

        var rndAuctions = DataFeed.Circular(auctions);
        var httpClient = Http.CreateDefaultClient();

        return Scenario.Create("ReadAuction_Scenario", async context =>
        {
            //получение токена доступа
            var getToken = await Step.Run("Получение_токена", context, async () =>
            {
                var login = Http.CreateRequest("POST", loginData.url)
                .WithHeader("Content-Type", "application/json")
                .WithJsonBody(loginData);
                return await Http.Send<ApiResponse<LoginResponseDTO>>(httpClient, login);
            });

            var auction = rndAuctions.GetNextItem(context.ScenarioInfo);

            //чтение содержания аукциона
            var readAuction = await Step.Run("Чтение_аукционов", context, async () =>
            {
                var request = Http.CreateRequest("GET", auction.Url + auction.Id)
                                .WithHeader("Accept", "application/json")
                                .WithHeader("TraceId", Guid.NewGuid().ToString())
                                .WithHeader("Requesttype", "ReadDetail")
                                .WithHeader("Authorization", $"Bearer {getToken.Payload.Value.Data.Result.Token}");
                return await Http.Send<ApiResponse<AuctionItem>>(httpClient, request);

            });

            //запрос имени автора аукциона
            var getSeller = await Step.Run("Чтение_автора", context, async () =>
            {
                identity.login = readAuction.Payload.Value.Data.Result.Seller;
                var request = Http.CreateRequest("POST", identity.url)
                                .WithHeader("Accept", "application/json")
                                .WithJsonBody(identity);
                return await Http.Send<ApiResponse<string>>(httpClient, request);
            });

            //запрос изображения
            var getImage = await Step.Run("Получение_изображения", context, async () =>
            {
                var request = Http.CreateRequest("GET", $"{image.url}?id={auction.Id}&cache=false")
                                .WithHeader("Accept", "application/json");
                return await Http.Send<ApiResponse<string>>(httpClient, request);
            });

            //сохранение истории открытия текущей страницы
            var setCurrentPage = await Step.Run("Сохранение_истории_открытия", context, async () =>
            {
                var request = Http.CreateRequest("POST", currentPage.url)
                                .WithHeader("Accept", "application/json")
                                .WithHeader("TraceId", Guid.NewGuid().ToString())
                                .WithHeader("Requesttype", "UsersCurrentPage")
                                .WithHeader("User", "admin")
                                .WithHeader("Authorization", $"Bearer {getToken.Payload.Value.Data.Result.Token}")
                                .WithJsonBody($"/auctions/{auction.Id}");
                return await Http.Send(httpClient, request);
            });

            //чтение ставок аукциона
            var readBids = await Step.Run("Чтение_ставок", context, async () =>
            {
                var request = Http.CreateRequest("GET", bids.url + auction.Id)
                                .WithHeader("Accept", "application/json")
                                .WithHeader("TraceId", Guid.NewGuid().ToString())
                                .WithHeader("Requesttype", "Bids")
                                .WithHeader("Authorization", $"Bearer {getToken.Payload.Value.Data.Result.Token}");
                return await Http.Send(httpClient, request);

            });

            return Response.Ok();
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.RampingConstant(copies: 800,
                                     during: TimeSpan.FromSeconds(30)),
            Simulation.RampingConstant(copies: 0,
                                     during: TimeSpan.FromSeconds(10))
        );
    }

}