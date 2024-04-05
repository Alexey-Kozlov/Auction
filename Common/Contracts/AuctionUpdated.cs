using System;

namespace Contracts;

public class AuctionUpdated
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Image { get; set; }
    public string Description { get; set; }
    public DateTime AuctionEnd { get; set; }
}
