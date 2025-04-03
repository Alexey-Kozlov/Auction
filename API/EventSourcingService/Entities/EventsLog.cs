using System.Text.Json;
using Common.Contracts;
using Common.Contracts.Processing;

namespace EventSourcingService.Entities;

public class EventsLog : IDisposable
{
#nullable enable
    public int Version { get; set; }
    public Guid CorrelationId { get; set; }
    public DateTime CreateAt { get; set; }
    public JsonDocument? EventData { get; set; }
    public bool Commited { get; set; }
    public string? Description { get; set; }
    public Guid? SnapShotId { get; set; }
    public string? EntityType { get; set; }
    public Guid? AuctionId { get; set; }
    public string? UserLogin { get; set; }
    public Command Command { get; set; }
    public CRUD CRUD { get; set; }
    public byte[]? Image { get; set; }
#nullable disable
    public void Dispose() => EventData?.Dispose();

}