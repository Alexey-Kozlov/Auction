﻿namespace AuctionService.DTO;

public class UpdateAuctionDTO
{
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public int ReservePrice { get; set; }
    public DateTime AuctionEnd { get; set; }
}
