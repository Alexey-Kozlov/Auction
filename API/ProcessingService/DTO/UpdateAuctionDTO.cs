namespace ProcessingService.DTO;
public class UpdateAuctionDTO
{
    public Guid AuctionId { get; set; }
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public int ReservePrice { get; set; }
    public DateTime AuctionEnd { get; set; }
    public Guid CorrelationId { get; set; }
    public bool UsingImage { get; set; }
}
