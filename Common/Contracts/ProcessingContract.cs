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
    private string _message;
    private T _sendObject;
    public FaultMessage(string message, T sendObject)
    {
        _correlationId = sendObject.CorrelationId;
        _message = message;
        _sendObject = sendObject;
    }
    public Guid FaultId => _correlationId;

    public Guid? FaultedMessageId => Guid.NewGuid();

    public DateTime Timestamp => DateTime.UtcNow;

    public ExceptionInfo[] Exceptions => [new FaultExceptionInfo(_message)];

    public HostInfo Host => new FaultHostInfo();

    public string[] FaultMessageTypes => [];

    public T Message => _sendObject;
}

public class FaultExceptionInfo : ExceptionInfo
{
    private string _message;
    public FaultExceptionInfo(string message)
    {
        _message = message;
    }
    public string ExceptionType => "FinanceException";

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

public class ESLog_AuctionDeleted
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
}
public class ESLog_AuctionCreated
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
}
public class ESLog_AuctionUpdated
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
}
public class ESLog_FinanceCreated
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
}
public class ESLog_PlaceBid : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
}
public class ESLog_ElkIndex
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int BatchCount { get; set; }
    public int AllItemsCount { get; set; }
}

public class ESLog_RestoreItems
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int BatchCount { get; set; }
}

public class ESLog_RestoreImages
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int BatchCount { get; set; }
    public int AllItemsCount { get; set; }
}

public class ESLog_EditNotification
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
}

public class ESLog_AuctionFinish
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
}

public class ESLog_ResetSnapShot
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }

}

public class ESLog_ProcessImages
{
    public Guid CorrelationId { get; set; }
    public int BatchCounter { get; set; }
}