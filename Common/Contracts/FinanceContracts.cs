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
public record FinanceCreating(
     int Amount,
     string UserLogin,
     Guid CorrelationId
);
public record FinanceCreated(Guid CorrelationId);
public record FinanceCreatingNotification(
     Guid CorrelationId,
     string SessionId,
     string UserLogin
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
public record FinanceAddCredit(List<FinanceCreateMessage> FinanceActionsList);

public record FinanceCorrectionStart(List<(FinanceItem, OperationType)> FinanceActionsList);
public record FinanceCorrectionEnd();
public enum FinanceRecordStatus
{
     Приход,
     Расход,
     Баланс
}
