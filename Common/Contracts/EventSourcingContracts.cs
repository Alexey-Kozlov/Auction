namespace Common.Contracts;

public class BaseStateContract
{
    public string Data { get; set; }
    public string Type { get; set; }
    public Guid CorrelationId { get; set; }
}

public class UpdateAuctionStateContract : BaseStateContract
{
    public string CurrentState { get; set; }
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Description { get; set; }
    public string AuctionAuthor { get; set; }
    public DateTime AuctionEnd { get; set; }
    public DateTime LastUpdated { get; set; }
    public string ErrorMessage { get; set; }
    public string Image { get; set; }
}

public class CommitAuctionUpdatingContract : BaseStateContract;

public record CommitAuctionUpdatedContract
(
    Guid CorrelationId
);

public class CreateAuctionStateContract : BaseStateContract
{
    public string CurrentState { get; set; }
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Description { get; set; }
    public string AuctionAuthor { get; set; }
    public DateTime AuctionEnd { get; set; }
    public DateTime LastUpdated { get; set; }
    public string ErrorMessage { get; set; }
    public int ReservePrice { get; set; }
    public string Image { get; set; }
}

public class CommitAuctionCreatingContract : BaseStateContract { }
public record CommitAuctionCreatedContract
(
    Guid CorrelationId
);

public class DeleteAuctionStateContract : BaseStateContract
{
    public string CurrentState { get; set; }
    public Guid Id { get; set; }
    public string AuctionAuthor { get; set; }
    public DateTime LastUpdated { get; set; }
    public string ErrorMessage { get; set; }
}

public class CommitAuctionDeletingContract : BaseStateContract { }

public record CommitAuctionDeletedContract
(
    Guid CorrelationId
);

public class BidPlacedStateContract : BaseStateContract
{
    public string CurrentState { get; set; }
    public string Bidder { get; set; }
    public Guid Id { get; set; }
    public int Amount { get; set; }
    public Guid BidId { get; set; }
    public int OldHighBid { get; set; }
    public DateTime LastUpdated { get; set; }
    public string ErrorMessage { get; set; }
}
public record AfterBidPlacedContract
(
    Guid CorrelationId
);

public class CommitBidPlacingContract : BaseStateContract { }

public record CommitBidPlacedContract
(
    Guid CorrelationId
);
public record CommitBidPlacedErrorContract
(
    Guid CorrelationId,
    Exception ExceptionItem
);

public class CommitBidErrorContract : BaseStateContract
{

}

public record CommitErrorSavedContract
(
    Guid CorrelationId
);

public class FinishAuctionStateContract : BaseStateContract
{
    public string CurrentState { get; set; }
    public Guid Id { get; set; }
    public string Winner { get; set; }
    public bool ItemSold { get; set; }
    public int Amount { get; set; }
    public DateTime LastUpdated { get; set; }
    public string ErrorMessage { get; set; }
}

public class CommitAuctionFinishingContract : BaseStateContract { }

public record CommitAuctionFinishedContract
(
    Guid CorrelationId
);
