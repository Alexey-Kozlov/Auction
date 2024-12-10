namespace Common.Contracts.Image;

public class ImageItem
{
    public Guid AuctionId { get; set; }
    public byte[] Image { get; set; }
}
public class ImageDTO
{
    public Guid AuctionId { get; set; }
    public string Image { get; set; }
}

public class ResetImageCache
{
    public string SessionId { get; set; }
}
public class ResetImageCacheNotification
{
    public string SessionId { get; set; }
}