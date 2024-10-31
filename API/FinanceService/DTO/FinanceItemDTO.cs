using Common.Contracts;

namespace FinanceService.DTO;

public class BalanceItemDTO
{
    public Guid? AuctionId { get; set; }
    public Guid ItemId { get; set; }
    public string UserLogin { get; set; }
    public int Value { get; set; }
    public DateTime ActionDate { get; set; }
    public FinanceRecordStatus Status { get; set; }
}