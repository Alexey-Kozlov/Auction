namespace Common.Contracts;

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

public record ElkIndexRequest(
     Guid Id,
     Guid CorrelationId,
     AuctionCreatingElk Item,
     bool LastItem,
     int ItemNumber,
     string SessionId
);

public record ElkIndexCreating(
     Guid CorrelationId,
     AuctionCreatingElk Item,
     int ItemNumber
);

public record ElkIndexCreated(
     Guid CorrelationId,
     ResultType Result
);

public record ElkIndexResponse(
     Guid CorrelationId,
     ResultType Result,
     bool LastItem,
     int ItemNumber,
     string SessionId
);

public record ElkIndexResponseCompleted(
     Guid CorrelationId
);

public enum ResultType
{
     Error,
     Success,
     EmptyResult
}