using System.Text.Json;
using Common.Contracts;

namespace EventSourcingService.Entities;

public class EventsLog : IDisposable
{
    public int Version { get; set; }
    public Guid CorrelationId { get; set; }
    public DateTime CreateAt { get; set; }
    public JsonDocument EventData { get; set; }
    public bool Commited { get; set; }
    public string Description { get; set; }
    public Guid? SnapShotId { get; set; }
    public string EntityType { get; set; }
    public int? RestoringOrder { get; set; }
    public Guid? AuctionId { get; set; }
    public string UserLogin { get; set; }
    public OperationType OperationType { get; set; }

    public void Dispose() => EventData?.Dispose();

}