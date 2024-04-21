namespace Contracts;

public record FinanceCreditAdd(
     decimal Credit,
     string UserLogin
);

public record FinanceDebitAdd(
    decimal Debit,
    Guid AuctionId,
    string UserLogin
);