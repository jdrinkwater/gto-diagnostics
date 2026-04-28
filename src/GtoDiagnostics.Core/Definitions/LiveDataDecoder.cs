namespace GtoDiagnostics.Core.Definitions;

/// <summary>
/// Decodes raw module responses using configured sensor definitions.
/// </summary>
public sealed class LiveDataDecoder
{
    private readonly VehicleModuleDefinition definition;

    /// <summary>
    /// Creates a decoder for the supplied module definition.
    /// </summary>
    /// <param name="definition">Sensor definition set.</param>
    public LiveDataDecoder(VehicleModuleDefinition definition)
    {
        this.definition = definition;
    }

    /// <summary>
    /// Decodes all configured sensors from a raw response.
    /// </summary>
    /// <param name="response">Raw module response bytes.</param>
    /// <returns>Decoded sensor readings.</returns>
    public IReadOnlyList<SensorReading> Decode(ReadOnlySpan<byte> response)
    {
        var readings = new List<SensorReading>(definition.Sensors.Count);

        foreach (var sensor in definition.Sensors)
        {
            readings.Add(sensor.Decode(response));
        }

        return readings;
    }
}
