using GtoDiagnostics.Core.Definitions;

namespace GtoDiagnostics.Core.Logging;

/// <summary>
/// A persisted decoded live-data reading.
/// </summary>
/// <param name="Timestamp">The UTC instant the reading was decoded.</param>
/// <param name="Module">The diagnostic module that produced the reading.</param>
/// <param name="SensorId">Stable sensor identifier.</param>
/// <param name="SensorName">Human-readable sensor name.</param>
/// <param name="Value">Decoded engineering value.</param>
/// <param name="Unit">Engineering unit.</param>
public sealed record DecodedReadingLogEntry(
    DateTimeOffset Timestamp,
    DiagnosticModule Module,
    string SensorId,
    string SensorName,
    double Value,
    string Unit)
{
    /// <summary>
    /// Creates a log entry from an in-memory sensor reading.
    /// </summary>
    /// <param name="timestamp">The UTC instant the reading was decoded.</param>
    /// <param name="module">The diagnostic module that produced the reading.</param>
    /// <param name="reading">Decoded sensor reading.</param>
    /// <returns>A persistable log entry.</returns>
    public static DecodedReadingLogEntry FromReading(
        DateTimeOffset timestamp,
        DiagnosticModule module,
        SensorReading reading)
    {
        return new DecodedReadingLogEntry(
            timestamp,
            module,
            reading.Id,
            reading.Name,
            reading.Value,
            reading.Unit);
    }
}
