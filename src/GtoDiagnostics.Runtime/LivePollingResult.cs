using GtoDiagnostics.Core.Definitions;

namespace GtoDiagnostics.Runtime;

/// <summary>
/// Result of one live-data polling cycle.
/// </summary>
/// <param name="Command">Bytes transmitted for the request.</param>
/// <param name="Response">Bytes received from the module.</param>
/// <param name="Readings">Decoded readings from the response.</param>
public sealed record LivePollingResult(
    byte[] Command,
    byte[] Response,
    IReadOnlyList<SensorReading> Readings);
