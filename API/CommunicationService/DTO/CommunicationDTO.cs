namespace CommunicationService.DTO;

public class CommunicationDTO
{
    public Guid ItemId { get; set; }
    public Guid? ParentId { get; set; }
    public string Message { get; set; }
    public string UserLogin { get; set; }
    public Guid AuctionId { get; set; }
    public DateTime UpdateAt { get; set; }
}