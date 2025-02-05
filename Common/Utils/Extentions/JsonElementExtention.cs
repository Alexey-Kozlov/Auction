using System.Text.Json;

namespace Common.Utils.Extentions;

public static class JsonElementExtention
{
    public static bool TryGetsString(this JsonElement je, out string parsed)
    {
        var (p, r) = je.ValueKind switch
        {
            JsonValueKind.String => (je.GetString(), true),
            JsonValueKind.Null => (null, true),
            _ => (default, false)
        };
        parsed = p;
        return r;
    }
}

