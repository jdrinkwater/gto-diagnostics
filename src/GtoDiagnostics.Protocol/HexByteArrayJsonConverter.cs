using System.Text.Json;
using System.Text.Json.Serialization;

namespace GtoDiagnostics.Protocol;

internal sealed class HexByteArrayJsonConverter : JsonConverter<byte[]>
{
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return HexBytes.Parse(reader.GetString() ?? string.Empty);
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected a hex string or byte array.");
        }

        var bytes = new List<byte>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return bytes.ToArray();
            }

            if (reader.TokenType != JsonTokenType.Number || !reader.TryGetByte(out var value))
            {
                throw new JsonException("Expected byte values in the array.");
            }

            bytes.Add(value);
        }

        throw new JsonException("Unexpected end of byte array.");
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(HexBytes.Format(value));
    }
}
