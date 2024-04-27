namespace Contracts;

public record UserNotificationSet(
     Guid AuctionId,
     string UserLogin,
     Guid CorrelationId
);

public record UserNotificationAdded(Guid CorrelationId);