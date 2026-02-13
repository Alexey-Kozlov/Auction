namespace ReportService.DTO.ResultDTO;

public class AuctionTreeItemsDTO
{
    public string Bidder { get; set; }
    public int Amount { get; set; }
    public Guid ItemId { get; set; }
    public string Seller { get; set; }
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}