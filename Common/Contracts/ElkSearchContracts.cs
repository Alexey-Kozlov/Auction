using Common.Contracts.Auction;

namespace Common.Contracts.ELKSearch;

public record ElkSearchRequest(
     Guid Id,
     Guid CorrelationId,
     string SearchTerm,
     int PageNumber,
     int PageSize,
     string SessionId
);

public record ElkSearchCreating(
     Guid Id,
     Guid CorrelationId,
     string SearchTerm,
     int PageNumber,
     int PageSize
);

public record ElkSearchCreated<T>(
     Guid CorrelationId,
     string SearchTerm,
     ResultType ResultType,
     T Result
);

public record ElkSearchResponse<T>(
     Guid CorrelationId,
     string SearchTerm,
     ResultType ResultType,
     T Result,
     string SessionId
);

public record ElkSearchResponseCompleted(
     Guid CorrelationId
);

public record RequestElkIndex(
     string UserLogin,
     Guid CorrelationId,
     string SessionId
);

public record ElkIndexCreating(
     Guid CorrelationId,
     AuctionCreatingElk Item,
     int ItemNumber
);

public record ElkIndexCreated(
     Guid CorrelationId
);
public class ElkIndexReset
{
     public Guid CorrelationId { get; set; }
}


public class ElkIndexResponse
{
     public Guid CorrelationId { get; set; }
     public int ItemNumber { get; set; }
     public string SessionId { get; set; }
}

public class ElkIndexCompleted
{
     public Guid CorrelationId { get; set; }
}
public class ElkIndexEnd
{
     public Guid CorrelationId { get; set; }
}
public class ElkIndexESCommit
{
     public Guid CorrelationId { get; set; }
}

public enum ResultType
{
     Error,
     Success,
     EmptyResult
}