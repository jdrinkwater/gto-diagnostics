namespace GtoDiagnostics.Serial;

/// <summary>
/// Finds serial-like devices available to the current machine.
/// </summary>
public interface ISerialPortDiscovery
{
    /// <summary>
    /// Lists candidate serial ports.
    /// </summary>
    /// <returns>Detected port descriptors.</returns>
    IReadOnlyList<SerialPortDescriptor> ListPorts();
}
