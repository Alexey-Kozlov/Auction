namespace AuctionService.Exceptions;

[Serializable]
internal class BidAuctionPlacingException : Exception
{
    public string UserLogin { get; }
    public int BidAmount { get; }
    public string AuctionId { get; }
    public string ErrorText { get; }
    public BidAuctionPlacingException(string bidder, int bidAmount, string auctionId, string errorText)
        : base($"Ошибка размещения ставки на аукционе, {errorText} AuctionId - {auctionId}, {bidAmount} для пользователя '{bidder}'")
    {
        this.UserLogin = bidder;
        this.BidAmount = bidAmount;
        this.AuctionId = auctionId;
        this.ErrorText = errorText;
    }
}
