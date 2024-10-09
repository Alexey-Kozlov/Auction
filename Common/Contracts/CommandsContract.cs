namespace Common.Contracts;

public abstract class BaseCommand
{
    public Guid Id { get; set; }
    public string EditUser { get; set; }
    public string Type { get; set; }
}

public class CreateAuctionCommand : BaseCommand
{
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Description { get; set; }
    public DateTime AuctionEnd { get; set; }
    public int ReservePrice { get; set; }
}

public class DeleteAuctionCommand : BaseCommand
{

}

public class UpdateAuctionCommand : BaseCommand
{
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Description { get; set; }
    public DateTime AuctionEnd { get; set; }
    public int ReservePrice { get; set; }
}