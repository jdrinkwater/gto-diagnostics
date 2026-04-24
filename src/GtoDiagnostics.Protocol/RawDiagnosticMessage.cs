namespace GtoDiagnostics.Protocol;

/// <summary>
/// Represents bytes exchanged with a diagnostic module.
/// </summary>
/// <param name="Timestamp">The UTC instant the bytes were observed.</param>
/// <param name="Direction">Whether the bytes were transmitted or received.</param>
/// <param name="Module">The module context for the bytes, for example <c>engine_ecu</c>.</param>
/// <param name="Bytes">The payload bytes.</param>
public sealed record RawDiagnosticMessage(
    DateTimeOffset Timestamp,
    RawMessageDirection Direction,
    string Module,
    byte[] Bytes);

/// <summary>
/// Direction of a raw diagnostic message.
/// </summary>
public enum RawMessageDirection
{
    /// <summary>Bytes sent from the app to the vehicle/interface.</summary>
    Transmit,

    /// <summary>Bytes received from the vehicle/interface.</summary>
    Receive,
}
