using GtoDiagnostics.Core;
using GtoDiagnostics.Core.Definitions;
using GtoDiagnostics.Core.Logging;
using GtoDiagnostics.Protocol;
using GtoDiagnostics.Simulator;

await using var transport = new ScriptedByteTransport();
transport.EnqueueResponse(HexBytes.Parse("90 01 55"));

await transport.OpenAsync();
await transport.WriteAsync(HexBytes.Parse("10 01"));

var buffer = new byte[8];
var count = await transport.ReadAsync(buffer);

AssertEqual(3, count);
AssertEqual("10 01", HexBytes.Format(transport.Writes[0]));
AssertEqual("90 01 55", HexBytes.Format(buffer.AsSpan(0, count)));

var capturePath = Path.Combine(Path.GetTempPath(), $"gto-replay-capture-test-{Guid.NewGuid():N}.jsonl");
await using (var captureWriter = new RawCaptureWriter(capturePath))
{
    await captureWriter.WriteAsync(new RawDiagnosticMessage(
        DateTimeOffset.UnixEpoch,
        RawMessageDirection.Transmit,
        DiagnosticModule.EngineEcu.ToString(),
        HexBytes.Parse("10 01")));
    await captureWriter.WriteAsync(new RawDiagnosticMessage(
        DateTimeOffset.UnixEpoch.AddMilliseconds(20),
        RawMessageDirection.Receive,
        DiagnosticModule.EngineEcu.ToString(),
        HexBytes.Parse("90 01 64 80 8E")));
}

var definition = KnownModuleDefinitions.CreateProvisionalEngineEcu();
var decoder = new LiveDataDecoder(definition);
var decodedLogPath = Path.Combine(Path.GetTempPath(), $"gto-replay-readings-test-{Guid.NewGuid():N}.jsonl");

await using (var decodedWriter = new DecodedReadingLogWriter(decodedLogPath))
{
    await foreach (var message in RawCaptureReader.ReadAllAsync(capturePath))
    {
        if (message.Direction != RawMessageDirection.Receive)
        {
            continue;
        }

        var readings = decoder.Decode(message.Bytes);
        await decodedWriter.WriteAsync(message.Timestamp, DiagnosticModule.EngineEcu, readings);
    }
}

var decodedLog = await File.ReadAllTextAsync(decodedLogPath);
AssertContains("coolant_temp", decodedLog);
AssertContains("throttle_position", decodedLog);
AssertContains("battery_voltage", decodedLog);

Console.WriteLine("GtoDiagnostics.E2E.Tests passed.");

static void AssertEqual<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
    }
}

static void AssertContains(string expected, string actual)
{
    if (!actual.Contains(expected, StringComparison.Ordinal))
    {
        throw new InvalidOperationException($"Expected text to contain '{expected}'.");
    }
}
