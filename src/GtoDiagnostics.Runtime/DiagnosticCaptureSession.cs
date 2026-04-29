using System.Text.Json;
using System.Text.Json.Serialization;
using GtoDiagnostics.Core.Definitions;
using GtoDiagnostics.Core.Logging;
using GtoDiagnostics.Protocol;

namespace GtoDiagnostics.Runtime;

/// <summary>
/// Owns capture session files, metadata, and log writers.
/// </summary>
public sealed class DiagnosticCaptureSession : IAsyncDisposable
{
    private static readonly JsonSerializerOptions ManifestJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private DiagnosticCaptureSession(
        CaptureSessionManifest manifest,
        RawCaptureWriter rawCaptureWriter,
        DecodedReadingLogWriter decodedReadingLogWriter,
        DecodedReadingCsvLogWriter decodedReadingCsvLogWriter)
    {
        Manifest = manifest;
        RawCaptureWriter = rawCaptureWriter;
        DecodedReadingLogWriter = decodedReadingLogWriter;
        DecodedReadingCsvLogWriter = decodedReadingCsvLogWriter;
    }

    /// <summary>Gets the session metadata and generated paths.</summary>
    public CaptureSessionManifest Manifest { get; }

    /// <summary>Gets the raw capture writer.</summary>
    public RawCaptureWriter RawCaptureWriter { get; }

    /// <summary>Gets the decoded JSONL readings writer.</summary>
    public DecodedReadingLogWriter DecodedReadingLogWriter { get; }

    /// <summary>Gets the decoded CSV readings writer.</summary>
    public DecodedReadingCsvLogWriter DecodedReadingCsvLogWriter { get; }

    /// <summary>
    /// Creates a capture session and writes its manifest.
    /// </summary>
    /// <param name="directory">Directory for generated files.</param>
    /// <param name="definition">Module definition being captured.</param>
    /// <param name="mode">Capture mode, for example simulator or serial.</param>
    /// <param name="notes">Optional operator notes.</param>
    /// <returns>The created capture session.</returns>
    public static async Task<DiagnosticCaptureSession> CreateAsync(
        string directory,
        VehicleModuleDefinition definition,
        string mode,
        string notes = "")
    {
        Directory.CreateDirectory(directory);

        var startedAt = DateTimeOffset.UtcNow;
        var sessionId = startedAt.ToString("yyyyMMdd-HHmmss-fff");
        var basePath = Path.Combine(directory, $"session-{sessionId}");
        var manifest = new CaptureSessionManifest(
            sessionId,
            startedAt,
            definition.VehicleFamily,
            definition.Module,
            mode,
            $"{basePath}.manifest.json",
            $"{basePath}.raw.jsonl",
            $"{basePath}.readings.jsonl",
            $"{basePath}.readings.csv",
            notes);

        await SaveManifestAsync(manifest).ConfigureAwait(false);

        return new DiagnosticCaptureSession(
            manifest,
            new RawCaptureWriter(manifest.RawCapturePath),
            new DecodedReadingLogWriter(manifest.DecodedReadingsPath),
            new DecodedReadingCsvLogWriter(manifest.DecodedCsvPath));
    }

    /// <summary>
    /// Writes a raw message using the session module context.
    /// </summary>
    /// <param name="direction">Message direction.</param>
    /// <param name="bytes">Message bytes.</param>
    public async Task WriteRawMessageAsync(RawMessageDirection direction, byte[] bytes)
    {
        await RawCaptureWriter.WriteAsync(new RawDiagnosticMessage(
            DateTimeOffset.UtcNow,
            direction,
            Manifest.Module.ToString(),
            bytes)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await RawCaptureWriter.DisposeAsync().ConfigureAwait(false);
        await DecodedReadingLogWriter.DisposeAsync().ConfigureAwait(false);
        await DecodedReadingCsvLogWriter.DisposeAsync().ConfigureAwait(false);
    }

    private static async Task SaveManifestAsync(CaptureSessionManifest manifest)
    {
        await using var stream = File.Create(manifest.ManifestPath);
        await JsonSerializer.SerializeAsync(stream, manifest, ManifestJsonOptions).ConfigureAwait(false);
    }
}
