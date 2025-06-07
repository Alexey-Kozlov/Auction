namespace NotificationService.DTO;

public class MessageChat
{
    public string Id { get; set; }
    public string ParentId { get; set; }
    public string Message { get; set; }
    public string UserLogin { get; set; }
    public string AuctionId { get; set; }
    public string SessionId { get; set; }
}