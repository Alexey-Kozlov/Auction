using Common.Contracts.Processing;

namespace Common.Contracts.Finance;

public record FinanceCreate(
     int Amount,
     string UserLogin
);
public record RequestCreateFinance(
     int Amount,
     string UserLogin,
     Guid CorrelationId
);

public class FinanceCreated : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
     public Guid ItemId { get; set; }
};

public class FinanceCreateComplete : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
     public Guid ItemId { get; set; }
};

public class FinanceNotificationCreated : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
     public Guid ItemId { get; set; }
};

public class FinanceCreateESCommit : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
     public Guid ItemId { get; set; }
};

public class FinanceCommit
{
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
     public string CallBackType { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public Guid ItemId { get; set; }
}

public class FinanceItem
{
     public Guid ItemId { get; set; }
     public Guid? AuctionId { get; set; }
     public string UserLogin { get; set; }
     public int Value { get; set; }
     public DateTime ActionDate { get; set; }
     public FinanceRecordStatus Status { get; set; }
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
}

public class FinanceHistoryItem
{
     public Guid ItemId { get; set; }
     public Guid? AuctionId { get; set; }
     public string UserLogin { get; set; }
     public int Value { get; set; }
     public DateTime ActionDate { get; set; }
     public FinanceRecordStatus Status { get; set; }
     public string AuctionTitle { get; set; }
     public string AuctionSeller { get; set; }
}

public class FinanceSortRequest
{
     public List<FinanceHistoryItem> FinanceItems { get; set; }
     public string OrderBy { get; set; }
     public string UserLogin { get; set; }
     public int PageNumber { get; set; }
     public int PageSize { get; set; }
}

public enum FinanceRecordStatus
{
     Приход,
     Расход,
     Баланс
}

public class FinanceReset : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
     public Guid ItemId { get; set; }
};
