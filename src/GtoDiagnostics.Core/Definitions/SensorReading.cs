namespace GtoDiagnostics.Core.Definitions;

/// <summary>
/// A decoded live-data value.
/// </summary>
/// <param name="Id">Stable sensor identifier.</param>
/// <param name="Name">Human-readable sensor name.</param>
/// <param name="Unit">Engineering unit.</param>
/// <param name="Value">Decoded value.</param>
public sealed record SensorReading(string Id, string Name, string Unit, double Value);
