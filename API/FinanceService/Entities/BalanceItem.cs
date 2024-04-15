namespace FinanceService.Entities;

public class BalanceItem
{
    public Guid Id { get; set; }
    public Guid? AuctionId { get; set; }
    public string UserLogin { get; set; }
    public decimal Credit { get; set; }
    public decimal Debit { get; set; }
    public DateTime ActionDate { get; set; }
    public bool Reserved { get; set; }
    public decimal Balance { get; set; }
}