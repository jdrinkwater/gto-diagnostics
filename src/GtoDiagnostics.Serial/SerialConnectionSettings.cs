namespace GtoDiagnostics.Serial;

/// <summary>
/// Serial settings used by a diagnostic interface.
/// </summary>
public sealed record SerialConnectionSettings
{
    /// <summary>The serial device path or Windows port name.</summary>
    public required string PortName { get; init; }

    /// <summary>The baud rate to use while the protocol is being characterised.</summary>
    public int BaudRate { get; init; } = 1953;

    /// <summary>Read timeout for transport operations.</summary>
    public TimeSpan ReadTimeout { get; init; } = TimeSpan.FromMilliseconds(500);

    /// <summary>Write timeout for transport operations.</summary>
    public TimeSpan WriteTimeout { get; init; } = TimeSpan.FromMilliseconds(500);
}
