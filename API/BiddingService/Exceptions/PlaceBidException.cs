namespace BiddingService.Exceptions;

[Serializable]
internal class PlaceBidException : Exception
{
    public string UserLogin { get; }
    public int BidAmount { get; }
    public string AuctionId { get; }
    public string ErrorText { get; }
    public PlaceBidException(string userLogin, int bidAmount, string auctionId, string errorText)
        : base($"Ошибка создания ставки на аукционе, {errorText} AuctionId - {auctionId}, {bidAmount} для пользователя '{userLogin}'")
    {
        this.UserLogin = userLogin;
        this.BidAmount = bidAmount;
        this.AuctionId = auctionId;
        this.ErrorText = errorText;
    }
}
