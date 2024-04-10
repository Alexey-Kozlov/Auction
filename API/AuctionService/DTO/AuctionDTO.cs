namespace AuctionService.DTO;

public record AuctionDTO
{
    public Guid Id { get; set; }
    public int ReservePrice { get; set; }
    public string Seller { get; set; }
    public string Winner { get; set; }
    public int SoldAmount { get; set; }
    public int CurrentHighBid { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime AuctionEnd { get; set; }
    public string Status { get; set; }
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
}