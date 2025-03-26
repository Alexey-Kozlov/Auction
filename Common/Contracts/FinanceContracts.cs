using Common.Contracts.Processing;

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

public class FinanceCreated : BaseServiceError, IFaultMessage { };

public class FinanceCreateComplete : BaseServiceError, IFaultMessage { };

public class FinanceNotificationCreated : BaseServiceError, IFaultMessage { };

public class FinanceCreateESCommit : BaseServiceError, IFaultMessage { };

public class FinanceError : BaseServiceError, IFaultMessage { };
public class FinanceCommit
{
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
     public string CallBackType { get; set; }
}

public class FinanceItem
{
     public Guid? Id { get; set; }
     public Guid FinanceId { get; set; }
     public Guid? AuctionId { get; set; }
     public string UserLogin { get; set; }
     public int Value { get; set; }
     public DateTime ActionDate { get; set; }
     public FinanceRecordStatus Status { get; set; }
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
}


public enum FinanceRecordStatus
{
     Приход,
     Расход,
     Баланс
}
