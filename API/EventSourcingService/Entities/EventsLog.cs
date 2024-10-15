using System.Text.Json;

namespace EventSourcingService.Entities;

public class EventsLog : IDisposable
{
    public int Version { get; set; }
    public Guid CommandId { get; set; }
    public DateTime CreateAt { get; set; }
    public JsonDocument EventData { get; set; }
    public bool Commited { get; set; }
    public string Info { get; set; }

    public void Dispose() => EventData?.Dispose();

}