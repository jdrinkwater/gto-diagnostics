using System.Globalization;

namespace GtoDiagnostics.Protocol;

/// <summary>
/// Converts diagnostic byte arrays to and from readable hexadecimal strings.
/// </summary>
public static class HexBytes
{
    /// <summary>
    /// Parses values like <c>10 20 FF</c> or <c>1020ff</c>.
    /// </summary>
    /// <param name="value">The hex string to parse.</param>
    /// <returns>The decoded bytes.</returns>
    public static byte[] Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var compact = value.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace(":", string.Empty, StringComparison.Ordinal);

        if (compact.Length % 2 != 0)
        {
            throw new FormatException("Hex byte strings must contain an even number of characters.");
        }

        var bytes = new byte[compact.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = byte.Parse(compact.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return bytes;
    }

    /// <summary>
    /// Formats bytes as upper-case hexadecimal separated by spaces.
    /// </summary>
    /// <param name="bytes">The bytes to format.</param>
    /// <returns>A readable hex string.</returns>
    public static string Format(ReadOnlySpan<byte> bytes)
    {
        return string.Join(' ', bytes.ToArray().Select(static b => b.ToString("X2", CultureInfo.InvariantCulture)));
    }
}
