using System.Text.Json;
using System.Text.Json.Serialization;

namespace QRWells.MapReduce.Rpc.Codecs;

public class JsonCodec : ICodec
{
    public async Task<string> EncodeAsync<T>(T obj)
    {
        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, obj);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    public async Task<T?> DecodeAsync<T>(Stream bytes)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new ObjectToInferredTypesConverter() }
        };
        return await JsonSerializer.DeserializeAsync<T>(bytes, options);
    }

    public class ObjectToInferredTypesConverter : JsonConverter<object>
    {
        public override object Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.Number when reader.TryGetInt32(out var i) => i,
                JsonTokenType.Number when reader.TryGetInt64(out var l) => l,
                JsonTokenType.Number => reader.GetDouble(),
                JsonTokenType.String => reader.GetString()!,
                _ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
            };
        }

        public override void Write(
            Utf8JsonWriter writer,
            object objectToWrite,
            JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, objectToWrite, objectToWrite.GetType(), options);
        }
    }
}