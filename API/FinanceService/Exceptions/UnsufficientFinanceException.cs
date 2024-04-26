namespace FinanceService.Exceptions;

[Serializable]
internal class UnsufficientFinanceException : Exception
{
    public string UserLogin { get; }
    public int BidAmount { get; }
    public UnsufficientFinanceException(string userLogin, int bidAmount)
        : base($"Недостаточно денег для ставки {bidAmount} для пользователя '{userLogin}'")
    {
        this.UserLogin = userLogin;
        this.BidAmount = bidAmount;
    }


}
