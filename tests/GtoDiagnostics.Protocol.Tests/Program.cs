using GtoDiagnostics.Protocol;

var bytes = HexBytes.Parse("10 20 ff");
AssertEqual(0x10, bytes[0]);
AssertEqual(0x20, bytes[1]);
AssertEqual(0xFF, bytes[2]);
AssertEqual("10 20 FF", HexBytes.Format(bytes));

var path = Path.Combine(Path.GetTempPath(), $"gto-protocol-test-{Guid.NewGuid():N}.jsonl");
await using (var writer = new RawCaptureWriter(path))
{
    await writer.WriteAsync(new RawDiagnosticMessage(
        DateTimeOffset.UnixEpoch,
        RawMessageDirection.Receive,
        "engine_ecu",
        [0x01, 0x02]));
}

var line = await File.ReadAllTextAsync(path);
if (!line.Contains("engine_ecu", StringComparison.OrdinalIgnoreCase) || !line.Contains("receive", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("Raw capture JSON did not contain expected module and direction fields.");
}

Console.WriteLine("GtoDiagnostics.Protocol.Tests passed.");

static void AssertEqual<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
    }
}
