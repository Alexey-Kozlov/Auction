using Common.Contracts.Auction;
using Common.Contracts.Processing;

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


public class RequestElkIndex
{
     public string UserLogin { get; set; }
     public Guid CorrelationId { get; set; }
     public string SessionId { get; set; }
     public string CallBackType { get; set; }
     public bool ShowMessages { get; set; }
}


public class ElkIndexResetRequest
{
     public Guid CorrelationId { get; set; }
     public string CallBackType { get; set; }
}

public class ElkIndexReset : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public bool IsError { get; set; }
}


public class ElkIndexCompleted : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public bool IsError { get; set; }
}


public class ElkIndexESCommit : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public bool IsError { get; set; }
}

public class ElkCommit
{
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
     public string CallBackType { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
}

public class ElkSearchResult : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public bool IsError { get; set; }
     public ApiResponse<PagedResult<List<AuctionCreatingElk>>> Result { get; set; }
}