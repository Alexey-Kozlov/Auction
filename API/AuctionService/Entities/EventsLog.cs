using System.Text.Json;

namespace AuctionService.Entities;

public class EventsLog : IDisposable
{
    public Guid Id { get; set; }
    public DateTime CreateAt { get; set; }
    public JsonDocument Aggregate { get; set; }

    public void Dispose() => Aggregate?.Dispose();

}