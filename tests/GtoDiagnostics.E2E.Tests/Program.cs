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

Console.WriteLine("GtoDiagnostics.E2E.Tests passed.");

static void AssertEqual<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
    }
}
