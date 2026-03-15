using Common.Contracts.Processing;

namespace Common.Contracts.Settings;

public class CurrentItem
{
     public Guid ItemId { get; set; }
     public bool AdminMode { get; set; }
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
}

public class RequestSetCurrentSettings
{
     public string UserLogin { get; set; }
     public Guid CorrelationId { get; set; }
     public bool AdminMode { get; set; }
}

public class SetCurrentSettings
{
     public string UserLogin { get; set; }
     public Guid CorrelationId { get; set; }
     public bool AdminMode { get; set; }
     public string CallBackType { get; set; }
}

public class SetCurrentSettingsCompleted : IFaultMessage
{
     public Guid CorrelationId { get; set; }

     public string UserLogin { get; set; }

     public string CallBackType { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
}

public class SetAdminMode
{
     public Guid CorrelationId { get; set; }
     public bool AdminMode { get; set; }
}