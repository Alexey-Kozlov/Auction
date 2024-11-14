using System.Reflection.Emit;
using MassTransit;

namespace Common.Contracts.Processing;

public record FaultTransfer(Guid CorrelationId, string Message);
public record FaultMessageSending(Guid CorrelationId, string Message, string UserLogin);
public class FaultMessageSended()
{
    public Guid CorrelationId { get; set; }
}

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
}

public enum Command
{
    AuctionDelete,  //0
    AuctionCreate,  //1
    AuctionUpdate,  //2
    FinanceCreate,  //3
    PlaceBid,       //4
    MakeSnapShot,   //5
    RestoreSnapShot //6
}

public enum CRUD
{
    Create,     //0
    Update,     //1
    Delete,     //2
    Read        //3
}

public record DataForProcessingServicesList<T>
(
    List<DataForProcessingService> DataObjects,
    Guid CorrelationId,
    string CallBackType = null
);

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
public class ESLog_PlaceBid
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
}
public class ESLog_MakeSnapShot
{
    public Guid CorrelationId { get; set; }
}
public class ESLog_RestoreSnapShot
{
    public Guid CorrelationId { get; set; }
}