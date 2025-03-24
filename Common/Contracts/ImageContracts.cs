namespace Common.Contracts.Image;

public class ImageItem
{
    public Guid? Id { get; set; }
    public Guid AuctionId { get; set; }
    public byte[] Image { get; set; }
    public Guid CorrelationId { get; set; }
    public bool Commited { get; set; }
}
public class ImageDTO
{
    public Guid? Id { get; set; }
    public Guid AuctionId { get; set; }
    public string Image { get; set; }
    public Guid CorrelationId { get; set; }
    public bool Commited { get; set; }
}

public class ResetImageCache
{
    public string SessionId { get; set; }
}
public class ResetImageCacheNotification
{
    public string SessionId { get; set; }
}

public class ImageCommit
{
    public Guid CorrelationId { get; set; }
    public bool Commited { get; set; }
    public string CallBackType { get; set; }
}