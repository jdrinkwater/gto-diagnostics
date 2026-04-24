using System.Text.Json.Serialization;

namespace GtoDiagnostics.Core.Definitions;

/// <summary>
/// External data definition for a diagnostic module.
/// </summary>
public sealed record VehicleModuleDefinition
{
    /// <summary>The module this definition applies to.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<DiagnosticModule>))]
    public DiagnosticModule Module { get; init; } = DiagnosticModule.EngineEcu;

    /// <summary>The vehicle family identifier, for example <c>gto_mk1</c>.</summary>
    public required string VehicleFamily { get; init; }

    /// <summary>Sensor definitions exposed by the module.</summary>
    public IReadOnlyList<SensorDefinition> Sensors { get; init; } = [];
}
