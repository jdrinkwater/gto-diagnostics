namespace GtoDiagnostics.Serial;

/// <summary>
/// Byte-oriented transport used below the vehicle protocol layer.
/// </summary>
public interface IByteTransport : IAsyncDisposable
{
    /// <summary>Gets whether the transport is currently open.</summary>
    bool IsOpen { get; }

    /// <summary>Opens the transport.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task OpenAsync(CancellationToken cancellationToken = default);

    /// <summary>Closes the transport.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CloseAsync(CancellationToken cancellationToken = default);

    /// <summary>Writes bytes to the transport.</summary>
    /// <param name="bytes">Bytes to transmit.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task WriteAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default);

    /// <summary>Reads up to <paramref name="buffer" /> length bytes.</summary>
    /// <param name="buffer">Destination buffer.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of bytes read.</returns>
    Task<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);
}
