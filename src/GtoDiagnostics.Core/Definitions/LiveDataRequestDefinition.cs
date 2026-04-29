namespace GtoDiagnostics.Core.Definitions;

/// <summary>
/// Groups sensors that can be decoded from one live-data request.
/// </summary>
/// <param name="Command">Hex command text used to request the grouped sensors.</param>
/// <param name="Sensors">Sensors decoded from the matching response.</param>
public sealed record LiveDataRequestDefinition(
    string Command,
    IReadOnlyList<SensorDefinition> Sensors);
