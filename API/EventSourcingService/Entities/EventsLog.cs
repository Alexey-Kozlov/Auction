using System.Text.Json;

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
    public string ServiceName { get; set; }
    public int LogicVersion { get; set; }
    public Guid? AuctionId { get; set; }
    public string UserLogin { get; set; }

    public void Dispose() => EventData?.Dispose();

}