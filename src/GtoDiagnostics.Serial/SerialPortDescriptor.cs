namespace GtoDiagnostics.Serial;

/// <summary>
/// A serial-like device that may be usable as a diagnostic interface.
/// </summary>
/// <param name="Name">The display name or device path.</param>
/// <param name="Description">Optional extra context for the device.</param>
public sealed record SerialPortDescriptor(string Name, string? Description = null);
