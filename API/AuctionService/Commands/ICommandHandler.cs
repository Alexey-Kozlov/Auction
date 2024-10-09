using Common.Contracts;

namespace AuctionService.Commands;

public interface ICommandHandler
{
    Task HandlerAsync(CreateAuctionCommand command);
    Task HandlerAsync(UpdateAuctionCommand command);
    Task HandlerAsync(DeleteAuctionCommand command);
}