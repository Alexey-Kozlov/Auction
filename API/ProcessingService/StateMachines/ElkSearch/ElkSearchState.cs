using MassTransit;

namespace ProcessingService.StateMachines.ElkSearchStateMachine;

public class ElkSearchState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Description { get; set; }
    public string AuctionAuthor { get; set; }
    public DateTime AuctionEnd { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime AuctionCreated { get; set; }
    public string ErrorMessage { get; set; }
    public string Winner { get; set; }
    public bool ItemSold { get; set; }
    public int Amount { get; set; }
    public int ReservePrice { get; set; }
    public string Term { get; set; }
    public int PgeNumber { get; set; }
    public int PageSize { get; set; }
    public string UserLogin { get; set; }
}
