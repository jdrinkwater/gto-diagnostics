using GtoDiagnostics.Core;

namespace GtoDiagnostics.Runtime;

/// <summary>
/// Describes the files and context for one diagnostic capture session.
/// </summary>
/// <param name="SessionId">Stable session identifier used in generated file names.</param>
/// <param name="StartedAt">UTC instant the session was started.</param>
/// <param name="VehicleFamily">Vehicle family identifier used by the module definition.</param>
/// <param name="Module">Diagnostic module captured by the session.</param>
/// <param name="Mode">Capture mode, for example simulator or serial.</param>
/// <param name="ManifestPath">Path to this manifest file.</param>
/// <param name="RawCapturePath">Path to the raw JSONL capture file.</param>
/// <param name="DecodedReadingsPath">Path to the decoded JSONL readings file.</param>
/// <param name="DecodedCsvPath">Path to the decoded CSV readings file.</param>
/// <param name="Notes">Optional operator notes.</param>
public sealed record CaptureSessionManifest(
    string SessionId,
    DateTimeOffset StartedAt,
    string VehicleFamily,
    DiagnosticModule Module,
    string Mode,
    string ManifestPath,
    string RawCapturePath,
    string DecodedReadingsPath,
    string DecodedCsvPath,
    string Notes);
