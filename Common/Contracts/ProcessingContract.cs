using MassTransit;

namespace Common.Contracts.Processing;

public record FaultMessageSending(Guid CorrelationId, string Message, string UserLogin);

public interface IFaultMessage
{
    public Guid CorrelationId { get; set; }
}

public class FaultMessage<T> : Fault<T> where T : IFaultMessage
{
    private Guid _correlationId;
    private T _sendObject;
    public FaultMessage(T sendObject)
    {
        _correlationId = sendObject.CorrelationId;
        _sendObject = sendObject;
    }
    public Guid FaultId => _correlationId;

    public Guid? FaultedMessageId => Guid.NewGuid();

    public DateTime Timestamp => DateTime.UtcNow;

    public ExceptionInfo[] Exceptions => [new FaultExceptionInfo("")];

    public HostInfo Host => new FaultHostInfo();

    public string[] FaultMessageTypes => [];

    public T Message => _sendObject;
    public Fault<T> CastItem()
    {
        return (Fault<T>)this;
    }
}

public class FaultExceptionInfo : ExceptionInfo
{
    private string _message;
    public FaultExceptionInfo(string message)
    {
        _message = message;
    }
    public string ExceptionType => "";

    public ExceptionInfo InnerException => null;

    public string StackTrace => "";

    public string Message => _message;

    public string Source => "";
    public IDictionary<string, object> Data => null;
}

public class FaultHostInfo : HostInfo
{
    public string MachineName => null;

    public string ProcessName => null;

    public int ProcessId => System.Diagnostics.Process.GetCurrentProcess().Id;

    public string Assembly => null;

    public string AssemblyVersion => null;

    public string FrameworkVersion => null;

    public string MassTransitVersion => null;

    public string OperatingSystemVersion => null;
}

public class DataForProcessingServicesList
{
    public List<DataForProcessingService> DataObjects { get; set; }

}

public class DataForProcessingService
{
    public Guid Id { get; set; }
    public string DataType { get; set; }
    public string Data { get; set; }
    public CRUD CRUD { get; set; }
    public Guid MessagePartId { get; set; }
    public int MessagePartNumber { get; set; }
    public int MessagePartSize { get; set; }
    public int MessagePartCounts { get; set; }
}

public enum Command
{
    AuctionDelete,  //0
    AuctionCreate,  //1
    AuctionUpdate,  //2
    FinanceCreate,  //3
    PlaceBid,       //4
    MakeSnapShot,   //5
    RestoreSnapShot,//6
    IndexELK,       //7
    EditNotification, //8
    AuctionFinished //9
}

public enum CRUD
{
    Create,     //0
    Update,     //1
    Delete,     //2
    Read        //3
}

public class DataForProcessingServicesList<T>
{
    public List<DataForProcessingService> DataObjects { get; set; }
    public Guid CorrelationId { get; set; }
    public string Props { get; set; }
    public string CallBackType { get; set; } = null;
};

public class ESLogAuctionDeleted : IFaultMessage
{

    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
}
public class ESLogAuctionCreated : IFaultMessage
{

    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
}
public class ESLogAuctionUpdated : IFaultMessage
{

    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
}
public class ESLogFinanceCreated : IFaultMessage
{

    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
}
public class ESLogPlaceBid : IFaultMessage
{

    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
}
public class ESLogElkIndex : IFaultMessage
{

    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int BatchCount { get; set; }
    public int AllItemsCount { get; set; }
}

public class ESLogRestoreItems : IFaultMessage
{

    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int BatchCount { get; set; }
}

public class ResetItems : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
}

public class ESLogRestoreImages : IFaultMessage
{

    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int BatchCount { get; set; }
    public int AllItemsCount { get; set; }
}

public class ESLogEditNotification : IFaultMessage
{

    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
}

public class ESLogAuctionFinish : IFaultMessage
{

    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
}

public class ESLogResetSnapShot : IFaultMessage
{

    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }

}

public class ESLogProcessImages : IFaultMessage
{

    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
    public int BatchCounter { get; set; }
}

public class BaseServiceError
{
    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
}

public class LoggingServiceError
{
    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public bool IsError { get; set; }
    public Guid? TraceId { get; set; }
}

public class NotificationServiceError
{
    public Guid CorrelationId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public string UserLogin { get; set; }
    public string ServiceName { get; set; }
    public Guid? AuctionId { get; set; }
    public Guid? TraceId { get; set; }
    public bool IsError { get; set; }
    public string SessionId { get; set; }
}