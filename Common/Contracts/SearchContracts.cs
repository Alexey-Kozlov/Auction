namespace Common.Contracts;

public record SearchRequest(
     string SearchTerm,
     string Props
);

public record SearchResponse(

     Guid CorrelationId,
     ResultType ResultType,
     List<Guid> ResultList
);

public enum ResultType
{
     Error,
     Success,
     EmptyResult
}