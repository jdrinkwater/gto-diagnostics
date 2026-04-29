using GtoDiagnostics.Core.Definitions;
using GtoDiagnostics.Core.Logging;
using GtoDiagnostics.Protocol;
using GtoDiagnostics.Serial;

namespace GtoDiagnostics.Runtime;

/// <summary>
/// Runs live-data polling cycles over a byte transport.
/// </summary>
public sealed class LivePollingSession
{
    private readonly IByteTransport transport;
    private readonly VehicleModuleDefinition definition;
    private readonly LiveDataDecoder decoder;
    private readonly RawCaptureWriter? rawCaptureWriter;
    private readonly DecodedReadingLogWriter? decodedReadingLogWriter;
    private readonly DecodedReadingCsvLogWriter? decodedReadingCsvLogWriter;

    /// <summary>
    /// Creates a live polling session.
    /// </summary>
    /// <param name="transport">Byte transport to poll.</param>
    /// <param name="definition">Module definition used for request planning and decoding.</param>
    /// <param name="rawCaptureWriter">Optional raw capture writer.</param>
    /// <param name="decodedReadingLogWriter">Optional decoded JSONL reading writer.</param>
    /// <param name="decodedReadingCsvLogWriter">Optional decoded CSV reading writer.</param>
    public LivePollingSession(
        IByteTransport transport,
        VehicleModuleDefinition definition,
        RawCaptureWriter? rawCaptureWriter = null,
        DecodedReadingLogWriter? decodedReadingLogWriter = null,
        DecodedReadingCsvLogWriter? decodedReadingCsvLogWriter = null)
    {
        this.transport = transport;
        this.definition = definition;
        this.rawCaptureWriter = rawCaptureWriter;
        this.decodedReadingLogWriter = decodedReadingLogWriter;
        this.decodedReadingCsvLogWriter = decodedReadingCsvLogWriter;
        decoder = new LiveDataDecoder(definition);
    }

    /// <summary>
    /// Polls the first configured live-data request.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The transmitted command, received response, and decoded readings.</returns>
    public async Task<LivePollingResult> PollOnceAsync(CancellationToken cancellationToken = default)
    {
        var request = definition.GetLiveDataRequests().FirstOrDefault()
            ?? throw new InvalidOperationException("The module definition does not contain a live-data request command.");

        var command = HexBytes.Parse(request.Command);
        var timestamp = DateTimeOffset.UtcNow;

        await transport.WriteAsync(command, cancellationToken).ConfigureAwait(false);
        await WriteRawCaptureAsync(timestamp, RawMessageDirection.Transmit, command).ConfigureAwait(false);

        var buffer = new byte[256];
        var count = await transport.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        var response = buffer[..count];
        await WriteRawCaptureAsync(timestamp, RawMessageDirection.Receive, response).ConfigureAwait(false);

        var readings = decoder.Decode(response);
        await WriteDecodedReadingsAsync(timestamp, readings).ConfigureAwait(false);

        return new LivePollingResult(command, response, readings);
    }

    private async Task WriteRawCaptureAsync(
        DateTimeOffset timestamp,
        RawMessageDirection direction,
        byte[] bytes)
    {
        if (rawCaptureWriter is null)
        {
            return;
        }

        await rawCaptureWriter.WriteAsync(new RawDiagnosticMessage(
            timestamp,
            direction,
            definition.Module.ToString(),
            bytes)).ConfigureAwait(false);
    }

    private async Task WriteDecodedReadingsAsync(
        DateTimeOffset timestamp,
        IReadOnlyList<SensorReading> readings)
    {
        if (decodedReadingLogWriter is not null)
        {
            await decodedReadingLogWriter.WriteAsync(
                timestamp,
                definition.Module,
                readings).ConfigureAwait(false);
        }

        if (decodedReadingCsvLogWriter is not null)
        {
            await decodedReadingCsvLogWriter.WriteAsync(
                timestamp,
                definition.Module,
                readings).ConfigureAwait(false);
        }
    }
}
