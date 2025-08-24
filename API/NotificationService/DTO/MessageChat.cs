namespace NotificationService.DTO;

public class MessageChat
{
    public string ItemId { get; set; }
    public string ParentId { get; set; }
    public string Message { get; set; }
    public string UserLogin { get; set; }
    public string AuctionId { get; set; }
    public ActionType ActionType { get; set; }
}

public enum ActionType
{
    create,
    read,
    update,
    delete,
}