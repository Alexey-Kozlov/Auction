namespace Common.Contracts;

public record FinanceCreate(
     int Amount,
     string UserLogin
);
public record RequestCreateFinance(
     int Amount,
     string UserLogin,
     Guid CorrelationId,
     string SessionId
);

public record FinanceCreated
{
     public Guid CorrelationId { get; set; }
}
public record FinanceCreatingNotification(
     int Amount,
     string UserLogin,
     Guid CorrelationId
);
public record FinanceNotificationCreated(Guid CorrelationId);
public record FinanceCreateESCommit(Guid CorrelationId);

public record CommitESFinanceOperation(Guid CorrelationId);

public class FinanceItem
{
     public Guid Id { get; set; }
     public Guid? AuctionId { get; set; }
     public string UserLogin { get; set; }
     public int Value { get; set; }
     public DateTime ActionDate { get; set; }
     public FinanceRecordStatus Status { get; set; }
}

public enum FinanceRecordStatus
{
     Приход,
     Расход,
     Баланс
}
