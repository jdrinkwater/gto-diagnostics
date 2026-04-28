using System.Text.Json;
using System.Text.Json.Serialization;
using GtoDiagnostics.Core.Definitions;

namespace GtoDiagnostics.Core.Logging;

/// <summary>
/// Writes decoded live-data readings as JSON lines.
/// </summary>
public sealed class DecodedReadingLogWriter : IAsyncDisposable
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly StreamWriter writer;

    /// <summary>
    /// Initializes a new decoded reading log writer.
    /// </summary>
    /// <param name="path">Destination JSONL path.</param>
    public DecodedReadingLogWriter(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".");
        writer = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read));
    }

    /// <summary>
    /// Appends decoded readings using a shared timestamp.
    /// </summary>
    /// <param name="timestamp">The UTC instant the readings were decoded.</param>
    /// <param name="module">The diagnostic module that produced the readings.</param>
    /// <param name="readings">Decoded sensor readings.</param>
    public async Task WriteAsync(
        DateTimeOffset timestamp,
        DiagnosticModule module,
        IEnumerable<SensorReading> readings)
    {
        foreach (var reading in readings)
        {
            var entry = DecodedReadingLogEntry.FromReading(timestamp, module, reading);
            var json = JsonSerializer.Serialize(entry, Options);
            await writer.WriteLineAsync(json).ConfigureAwait(false);
        }

        await writer.FlushAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await writer.DisposeAsync().ConfigureAwait(false);
    }
}
