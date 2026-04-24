namespace GtoDiagnostics.Serial;

/// <summary>
/// Discovers common Linux USB serial devices without depending on a serial package.
/// </summary>
public sealed class LinuxSerialPortDiscovery : ISerialPortDiscovery
{
    private static readonly string[] Patterns = ["/dev/ttyUSB*", "/dev/ttyACM*", "/dev/serial/by-id/*"];

    /// <inheritdoc />
    public IReadOnlyList<SerialPortDescriptor> ListPorts()
    {
        if (!OperatingSystem.IsLinux())
        {
            return [];
        }

        return Patterns
            .SelectMany(static pattern =>
            {
                var directory = Path.GetDirectoryName(pattern) ?? "/";
                var filePattern = Path.GetFileName(pattern);
                return Directory.Exists(directory)
                    ? Directory.EnumerateFiles(directory, filePattern)
                    : [];
            })
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(static path => new SerialPortDescriptor(path))
            .ToArray();
    }
}
