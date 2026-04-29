using System.Text.Json;
using System.Text.Json.Serialization;

namespace GtoDiagnostics.Protocol;

internal static class RawCaptureJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter(),
            new HexByteArrayJsonConverter(),
        },
    };
}
