using System.Collections.Concurrent;
using GtoDiagnostics.Serial;

namespace GtoDiagnostics.Simulator;

/// <summary>
/// In-memory transport that returns queued responses for hardware-free tests and replay tools.
/// </summary>
public sealed class ScriptedByteTransport : IByteTransport
{
    private readonly ConcurrentQueue<byte[]> responses = new();
    private readonly List<byte[]> writes = [];

    /// <inheritdoc />
    public bool IsOpen { get; private set; }

    /// <summary>Gets bytes written by the caller.</summary>
    public IReadOnlyList<byte[]> Writes => writes;

    /// <summary>
    /// Adds a response to be returned by the next read call.
    /// </summary>
    /// <param name="bytes">Response bytes.</param>
    public void EnqueueResponse(ReadOnlySpan<byte> bytes)
    {
        responses.Enqueue(bytes.ToArray());
    }

    /// <inheritdoc />
    public Task OpenAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsOpen = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task CloseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsOpen = false;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task WriteAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureOpen();
        writes.Add(bytes.ToArray());
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureOpen();

        if (!responses.TryDequeue(out var response))
        {
            return Task.FromResult(0);
        }

        var count = Math.Min(response.Length, buffer.Length);
        response.AsMemory(0, count).CopyTo(buffer);
        return Task.FromResult(count);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        IsOpen = false;
        return ValueTask.CompletedTask;
    }

    private void EnsureOpen()
    {
        if (!IsOpen)
        {
            throw new InvalidOperationException("Transport is not open.");
        }
    }
}
