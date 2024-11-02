namespace Common.Contracts;

public class NotifyItem
{
     public Guid AuctionId { get; set; }
     public string UserLogin { get; set; }
}
public record UserNotificationSet(
     Guid AuctionId,
     string UserLogin,
     Guid CorrelationId
);

public record UserNotificationAdded(Guid CorrelationId);