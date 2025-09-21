namespace ReportService.DTO;

public class AuctionTreeItem
{
    public Guid key { get; set; }
    public AuctionTreeItemData data { get; set; }
    public BidTreeItem[] children { get; set; }
}

public class AuctionTreeItemData
{
    public string seller { get; set; }
    public string title { get; set; }
    public DateTime createAt { get; set; }
    public DateTime auctionEnd { get; set; }
    public Guid itemid { get; set; }
}

public class BidTreeItem
{
    public Guid key { get; set; }
    public BidTreeItemData data { get; set; }
}

public class BidTreeItemData
{
    public DateTime createAt { get; set; }
    public string bidder { get; set; }
    public int amount { get; set; }
}

