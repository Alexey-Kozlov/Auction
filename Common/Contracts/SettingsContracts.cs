namespace Common.Contracts.Settings;

public class CurrentItem
{
     public Guid ItemId { get; set; }
     public bool AdminMode { get; set; }
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
}
