namespace GtoDiagnostics.Core.Definitions;

/// <summary>
/// Describes how to request and scale one sensor value.
/// </summary>
public sealed record SensorDefinition
{
    /// <summary>Stable identifier used in logs.</summary>
    public required string Id { get; init; }

    /// <summary>Human-readable display name.</summary>
    public required string Name { get; init; }

    /// <summary>Display unit, for example <c>C</c>, <c>rpm</c>, or <c>V</c>.</summary>
    public required string Unit { get; init; }

    /// <summary>Hex command bytes used to request this value when known.</summary>
    public string? Command { get; init; }

    /// <summary>Zero-based byte position in the response.</summary>
    public int ByteIndex { get; init; }

    /// <summary>Multiplier used by the initial linear scaling model.</summary>
    public double Multiplier { get; init; } = 1;

    /// <summary>Offset used by the initial linear scaling model.</summary>
    public double Offset { get; init; }

    /// <summary>
    /// Scales a raw response into an engineering value.
    /// </summary>
    /// <param name="response">The raw module response.</param>
    /// <returns>The scaled sensor value.</returns>
    public SensorReading Decode(ReadOnlySpan<byte> response)
    {
        if (ByteIndex < 0 || ByteIndex >= response.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(response), "Response does not contain the configured byte index.");
        }

        return new SensorReading(Id, Name, Unit, (response[ByteIndex] * Multiplier) + Offset);
    }

}
