namespace ImageService.Entities;

public class ImageItem
{
    public Guid AuctionId { get; set; }
    public byte[] Image { get; set; }
}