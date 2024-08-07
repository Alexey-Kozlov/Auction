namespace Common.Contracts;

public record ElkSearchRequest(
     Guid Id,
     Guid CorrelationId,
     string SearchTerm,
     int PageNumber,
     int PageSize,
     string UserLogin
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
     string UserLogin
);

public record ElkSearchResponseCompleted(
     Guid CorrelationId
);

public enum ResultType
{
     Error,
     Success,
     EmptyResult
}