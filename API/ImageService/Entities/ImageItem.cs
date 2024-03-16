namespace ImageService.Entities;

public class ImageItem
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public byte[] Image { get; set; }
}