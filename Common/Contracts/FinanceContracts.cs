namespace Common.Contracts.Finance;

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
public record FinanceCreateComplete
{
     public Guid CorrelationId { get; set; }

}

public record FinanceNotificationCreated
{
     public Guid CorrelationId { get; set; }
}
public class FinanceCreateESCommit
{
     public Guid CorrelationId { get; set; }
}


public class FinanceItem
{
     public Guid FinanceId { get; set; }
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
