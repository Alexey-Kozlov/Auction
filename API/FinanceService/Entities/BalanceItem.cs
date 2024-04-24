namespace FinanceService.Entities;

public class BalanceItem
{
    public Guid Id { get; set; }
    public Guid? AuctionId { get; set; }
    public string UserLogin { get; set; }
    public int Credit { get; set; }
    public int Debit { get; set; }
    public DateTime ActionDate { get; set; }
    public bool Reserved { get; set; }
    public int Balance { get; set; }
}