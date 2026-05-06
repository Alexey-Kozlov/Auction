using Common.Contracts.Auction;
using Common.Contracts.Processing;

namespace Common.Contracts.ELKSearch;

public record ElkSearchRequest(
     Guid ItemId,
     string SearchTerm,
     int PageNumber,
     int PageSize,
     string UserLogin,
     string OrderBy,
     string[] AdvSearchParam
);


public class ElkIndexRequest
{
     public string UserLogin { get; set; }
     public Guid CorrelationId { get; set; }
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
     public string ErrorExceptionStack { get; set; }
     public string ErrorExceptionInputData { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }

}


public class ElkIndexCompleted : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionStack { get; set; }
     public string ErrorExceptionInputData { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }

}


public class ElkIndexESCommit : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionStack { get; set; }
     public string ErrorExceptionInputData { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }

}

public class ElkCommit
{
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
     public string ElkIndex { get; set; }
     public string CallBackType { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionStack { get; set; }
     public string ErrorExceptionInputData { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
}

public class ElkSearchResult
{
     public string ErrorMessage { get; set; }
     public string ErrorExceptionStack { get; set; }
     public string ErrorExceptionInputData { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }

     public ApiResponse<PagedResult<List<AuctionCreatingElk>>> Result { get; set; }

}