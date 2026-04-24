using GtoDiagnostics.Core;
using GtoDiagnostics.Protocol;
using GtoDiagnostics.Serial;
using GtoDiagnostics.Simulator;

if (args is ["ports"])
{
    var ports = new LinuxSerialPortDiscovery().ListPorts();
    if (ports.Count == 0)
    {
        Console.WriteLine("No Linux USB serial devices found.");
        return;
    }

    foreach (var port in ports)
    {
        Console.WriteLine(port.Name);
    }

    return;
}

if (args is ["simulate-capture", var outputPath])
{
    await using var capture = new RawCaptureWriter(outputPath);
    await using var transport = new ScriptedByteTransport();

    var command = HexBytes.Parse("10 01");
    var response = HexBytes.Parse("90 01 55");
    transport.EnqueueResponse(response);

    await transport.OpenAsync();
    await transport.WriteAsync(command);
    await capture.WriteAsync(new RawDiagnosticMessage(DateTimeOffset.UtcNow, RawMessageDirection.Transmit, DiagnosticModule.EngineEcu.ToString(), command));

    var buffer = new byte[16];
    var count = await transport.ReadAsync(buffer);
    await capture.WriteAsync(new RawDiagnosticMessage(DateTimeOffset.UtcNow, RawMessageDirection.Receive, DiagnosticModule.EngineEcu.ToString(), buffer[..count]));

    Console.WriteLine($"Wrote simulated raw capture to {outputPath}");
    return;
}

Console.WriteLine("GTO Diagnostics");
Console.WriteLine("Commands:");
Console.WriteLine("  ports");
Console.WriteLine("  simulate-capture <output.jsonl>");
