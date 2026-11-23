using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Screen.Core;

/// <summary>
/// The JSON options for every contract object (<c>Screen.Core.Contracts</c>). Use them for each read and write.
/// They refuse what the schemas refuse in structure: a missing or null required field, an unknown field
/// (<c>required</c>, <c>additionalProperties: false</c>) and a null list element.
/// They read and write each date-time in UTC with a <c>Z</c> offset, as the schemas require.
/// They do not check ranges, lengths or other patterns. Board B validates each object against its schema before it sends it.
/// </summary>
public static class ContractJson
{
    /// <summary>Strict options: required constructor parameters, nullable annotations, no unmapped members,
    /// no null list elements, UTC date-times with <c>Z</c>.</summary>
    public static JsonSerializerOptions Options { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.General)
        {
            RespectRequiredConstructorParameters = true,
            RespectNullableAnnotations = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            Converters = { new UtcInstantConverter(), new NonNullListConverterFactory() },
        };
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }

    /// <summary>Reads only a date-time that ends in <c>Z</c>. Writes UTC with <c>Z</c> and no trailing zero digits.</summary>
    private sealed class UtcInstantConverter : JsonConverter<DateTimeOffset>
    {
        private const string Format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFF'Z'";

        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string text = reader.GetString() ?? throw new JsonException("a date-time is null");
            if (!text.EndsWith('Z'))
            {
                throw new JsonException($"date-time '{text}' does not end in Z (UTC)");
            }

            return reader.GetDateTimeOffset();
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(writer);
            writer.WriteStringValue(value.UtcDateTime.ToString(Format, CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Handles <c>IReadOnlyList&lt;T&gt;</c> of a reference type and refuses a null element.
    /// No contract list holds null (no schema <c>items</c> allows it). Change this when one does.
    /// </summary>
    private sealed class NonNullListConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)
            && !typeToConvert.GetGenericArguments()[0].IsValueType;

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter)Activator.CreateInstance(
                typeof(NonNullListConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0]))!;
    }

    private sealed class NonNullListConverter<T> : JsonConverter<IReadOnlyList<T>>
        where T : class
    {
        public override IReadOnlyList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // List<T> is not IReadOnlyList<T>, so this does not call the factory again.
            List<T?> items = JsonSerializer.Deserialize<List<T?>>(ref reader, options)
                ?? throw new JsonException($"a list of {typeof(T).Name} is null");
            if (items.Contains(null))
            {
                throw new JsonException($"a list of {typeof(T).Name} holds a null element");
            }

            return items!;
        }

        public override void Write(Utf8JsonWriter writer, IReadOnlyList<T> value, JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(writer);
            ArgumentNullException.ThrowIfNull(value);
            writer.WriteStartArray();
            foreach (T item in value)
            {
                JsonSerializer.Serialize(writer, item, options);
            }

            writer.WriteEndArray();
        }
    }
}
