using System.Text.Json;
using System.Text.Json.Serialization;
using QRWells.MapReduce.Rpc.Extensions;

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

    public dynamic ExtractResult(object? o, Type returnType)
    {
        var dict = o as Dictionary<string, object?>;
        if (returnType == typeof(Task)) return Task.CompletedTask;
        var isTask = returnType.GetGenericTypeDefinition() == typeof(Task<>);
        var rawObj = isTask
            ? dict["Result"]
            : dict;
        var type = isTask ? returnType.GetGenericArguments()[0] : returnType;
        var t = ((IDictionary<string, object>)rawObj).ToObject(type);
        // convert to the correct type
        return isTask ? Task.FromResult(t) : t;
    }

    private class ObjectToInferredTypesConverter : JsonConverter<object>
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
                JsonTokenType.StartArray => JsonSerializer.Deserialize<List<object>>(ref reader, options)!,
                JsonTokenType.StartObject =>
                    JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options)!,
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