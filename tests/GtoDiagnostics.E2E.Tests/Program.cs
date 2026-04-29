using GtoDiagnostics.Core;
using GtoDiagnostics.Core.Definitions;
using GtoDiagnostics.Core.Logging;
using GtoDiagnostics.Protocol;
using GtoDiagnostics.Runtime;
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

await using var pollingTransport = new ScriptedByteTransport();
pollingTransport.EnqueueResponse(HexBytes.Parse("90 01 64 80 8E"));
await pollingTransport.OpenAsync();

var pollingSessionDirectory = Path.Combine(Path.GetTempPath(), $"gto-polling-session-test-{Guid.NewGuid():N}");
CaptureSessionManifest manifest;

await using (var captureSession = await DiagnosticCaptureSession.CreateAsync(
    pollingSessionDirectory,
    definition,
    "Simulator",
    "E2E test"))
{
    manifest = captureSession.Manifest;
    var session = new LivePollingSession(
        pollingTransport,
        definition,
        captureSession.RawCaptureWriter,
        captureSession.DecodedReadingLogWriter,
        captureSession.DecodedReadingCsvLogWriter);

    var result = await session.PollOnceAsync();
    AssertEqual("10 01", HexBytes.Format(result.Command));
    AssertEqual("90 01 64 80 8E", HexBytes.Format(result.Response));
    AssertEqual(3, result.Readings.Count);
    AssertEqual("coolant_temp", result.Readings[0].Id);
}

var manifestJson = await File.ReadAllTextAsync(manifest.ManifestPath);
AssertContains("gto_mk1", manifestJson);
AssertContains("EngineEcu", manifestJson);
AssertContains("Simulator", manifestJson);
AssertContains("E2E test", manifestJson);
AssertContains(manifest.RawCapturePath.Replace("\\", "\\\\", StringComparison.Ordinal), manifestJson);
AssertContains(manifest.DecodedReadingsPath.Replace("\\", "\\\\", StringComparison.Ordinal), manifestJson);
AssertContains(manifest.DecodedCsvPath.Replace("\\", "\\\\", StringComparison.Ordinal), manifestJson);

var pollingCapture = await File.ReadAllTextAsync(manifest.RawCapturePath);
AssertContainsIgnoreCase("\"direction\":\"transmit\"", pollingCapture);
AssertContainsIgnoreCase("\"direction\":\"receive\"", pollingCapture);
AssertContains("\"bytes\":\"10 01\"", pollingCapture);
AssertContains("\"bytes\":\"90 01 64 80 8E\"", pollingCapture);

var pollingJson = await File.ReadAllTextAsync(manifest.DecodedReadingsPath);
AssertContains("coolant_temp", pollingJson);
AssertContains("battery_voltage", pollingJson);

var pollingCsv = await File.ReadAllTextAsync(manifest.DecodedCsvPath);
AssertContains("timestamp,module,sensor_id,sensor_name,value,unit", pollingCsv);
AssertContains("EngineEcu,coolant_temp,Coolant Temperature,60,C", pollingCsv);

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

static void AssertContainsIgnoreCase(string expected, string actual)
{
    if (!actual.Contains(expected, StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"Expected text to contain '{expected}'.");
    }
}
