using Common.Contracts.Processing;

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

public class ImageReset : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
};

public class ImageReturnTypeSql
{
#nullable enable
    public Guid? id { get; set; }
    public byte[]? image { get; set; }
    public Guid? auctionid { get; set; }
    public int? recordscount { get; set; }
#nullable disable
}
