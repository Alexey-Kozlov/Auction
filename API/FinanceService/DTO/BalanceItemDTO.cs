namespace FinanceService.DTO;

public class BalanceItemDTO
{
    public Guid? Id { get; set; }
    public Guid ItemId { get; set; }
    public string UserLogin { get; set; }
    public int Credit { get; set; }
    public int Debit { get; set; }
    public DateTime ActionDate { get; set; }
    public RecordStatus Status { get; set; }
    public int Balance { get; set; }
}