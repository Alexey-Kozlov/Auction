namespace ReportService.DTO;

public class AuctionTreeItem
{
    public Guid key { get; set; }
    public AuctionTreeItemData data { get; set; }
    public List<BidTreeItem> children { get; set; } = new();
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

public class AuctionTreeItemCommunication
{
    public Guid key { get; set; }
    public AuctionTreeItemData data { get; set; }
    public List<CommunicationTreeItem> children { get; set; } = new();
}

public class CommunicationTreeItem
{
    public Guid key { get; set; }
    public CommunicationTreeItemData data { get; set; }
}

public class CommunicationTreeItemData
{
    public DateTime createAt { get; set; }
    public string author { get; set; }
    public string comment { get; set; }
}

