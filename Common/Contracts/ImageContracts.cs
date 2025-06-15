using Common.Contracts.Processing;

namespace Common.Contracts.Image;

public class ImageItem
{
    public Guid? ItemId { get; set; }
    public Guid AuctionId { get; set; }
    public byte[] Image { get; set; }
    public Guid CorrelationId { get; set; }
    public bool Commited { get; set; }
}
public class ImageDTO
{
    public Guid? ItemId { get; set; }
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
    public string ErrorMessage { get; set; }
    public string ErrorExceptionMessage { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
}

public class ImageReset : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionMessage { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
};

public class ImageReturnTypeSql
{
#nullable enable
    public Guid? ItemId { get; set; }
    public byte[]? image { get; set; }
    public Guid? auctionid { get; set; }
    public int? recordscount { get; set; }
#nullable disable
}
