using System.Text.Json;
using Grpc.Core;
using NotifyService;

namespace GatewayService.Services;

public class GrpcNotifyUsersService : NotifyUsers.NotifyUsersBase
{
    private readonly UserCurrentPage _userCurrentPage;
    public GrpcNotifyUsersService(UserCurrentPage userCurrentPage)
    {
        _userCurrentPage = userCurrentPage;
    }

    // получение списка пользователей, что открыли указанную страницу
    public override async Task<NotifyUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
    {
        var response = new NotifyUsersResponse
        {
            Users = JsonSerializer.Serialize(await _userCurrentPage.GetUsersForCurrentPage(request.PageId))
        };
        return response;
    }
}
