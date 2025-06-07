namespace Common.Utils.Logging;

public class GetErrorMessage
{
    public static Exception GetMessage(Exception e)
    {
        return new Exception(GetInnerException(e).Message, e);
    }

    public static Exception GetInnerException(Exception e)
    {
        if (e.InnerException != null)
        {
            return GetInnerException(e.InnerException);
        }
        else
        {
            return e;
        }
    }
}