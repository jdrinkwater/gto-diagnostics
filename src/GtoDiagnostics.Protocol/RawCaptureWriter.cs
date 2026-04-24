using System.Text.Json;
using System.Text.Json.Serialization;

namespace GtoDiagnostics.Protocol;

/// <summary>
/// Writes raw diagnostic traffic as JSON lines for reverse engineering.
/// </summary>
public sealed class RawCaptureWriter : IAsyncDisposable
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly StreamWriter writer;

    /// <summary>
    /// Initializes a new raw capture writer.
    /// </summary>
    /// <param name="path">Destination JSONL path.</param>
    public RawCaptureWriter(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".");
        writer = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read));
    }

    /// <summary>
    /// Appends one raw traffic message.
    /// </summary>
    /// <param name="message">Message to write.</param>
    public async Task WriteAsync(RawDiagnosticMessage message)
    {
        var json = JsonSerializer.Serialize(message, Options);
        await writer.WriteLineAsync(json).ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await writer.DisposeAsync().ConfigureAwait(false);
    }
}
