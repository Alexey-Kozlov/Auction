namespace Contracts;

public record FinanceCreditAdd(
     int Credit,
     string UserLogin
);

public record RequestFinanceDebitAdd(
    int Debit,
    Guid AuctionId,
    string UserLogin,
    Guid CorrelationId
);

public record FinanceGranted(Guid CorrelationId);