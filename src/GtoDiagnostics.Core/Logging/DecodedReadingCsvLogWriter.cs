using System.Globalization;
using GtoDiagnostics.Core.Definitions;

namespace GtoDiagnostics.Core.Logging;

/// <summary>
/// Writes decoded live-data readings as CSV rows.
/// </summary>
public sealed class DecodedReadingCsvLogWriter : IAsyncDisposable
{
    private const string Header = "timestamp,module,sensor_id,sensor_name,value,unit";
    private readonly StreamWriter writer;

    /// <summary>
    /// Initializes a new decoded reading CSV writer.
    /// </summary>
    /// <param name="path">Destination CSV path.</param>
    public DecodedReadingCsvLogWriter(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".");
        writer = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read));
        writer.WriteLine(Header);
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
        var timestampText = timestamp.ToString("O", CultureInfo.InvariantCulture);
        var moduleText = module.ToString();

        foreach (var reading in readings)
        {
            await writer.WriteLineAsync(string.Join(
                ',',
                Escape(timestampText),
                Escape(moduleText),
                Escape(reading.Id),
                Escape(reading.Name),
                Escape(reading.Value.ToString("G17", CultureInfo.InvariantCulture)),
                Escape(reading.Unit))).ConfigureAwait(false);
        }

        await writer.FlushAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await writer.DisposeAsync().ConfigureAwait(false);
    }

    private static string Escape(string value)
    {
        if (value.IndexOfAny([',', '"', '\r', '\n']) < 0)
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }
}
