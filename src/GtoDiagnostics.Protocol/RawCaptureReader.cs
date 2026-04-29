using System.Runtime.CompilerServices;
using System.Text.Json;

namespace GtoDiagnostics.Protocol;

/// <summary>
/// Reads raw diagnostic traffic from JSON lines capture files.
/// </summary>
public static class RawCaptureReader
{
    /// <summary>
    /// Streams raw diagnostic messages from a JSONL capture file.
    /// </summary>
    /// <param name="path">Source JSONL path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Messages in capture order.</returns>
    public static async IAsyncEnumerable<RawDiagnosticMessage> ReadAllAsync(
        string path,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(File.OpenRead(path));

        while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var message = JsonSerializer.Deserialize<RawDiagnosticMessage>(line, RawCaptureJson.Options)
                ?? throw new InvalidDataException($"Capture file '{path}' contained an empty message.");

            yield return message;
        }
    }
}
