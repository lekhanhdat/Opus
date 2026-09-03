using System;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LS.Converters
{
    /// <summary>
    /// Converts a non-generic <see cref="Hashtable"/> to/from JSON.
    /// Each value is tagged with its assembly-qualified type name so that
    /// the original CLR type can be restored on deserialization, since
    /// System.Text.Json cannot infer the runtime type of a boxed object.
    /// The rebuilt table uses <see cref="StringComparer.OrdinalIgnoreCase"/>
    /// to preserve the case-insensitive key semantics that the original
    /// BinaryFormatter-serialized hashtables relied on.
    /// </summary>
    public sealed class HashtableJsonConverter : JsonConverter<Hashtable>
    {
        private const string TypePropertyName = "$type";
        private const string ValuePropertyName = "$value";

        public override Hashtable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            Hashtable table = new Hashtable(StringComparer.OrdinalIgnoreCase);
            using JsonDocument doc = JsonDocument.ParseValue(ref reader);

            foreach (JsonProperty entryProperty in doc.RootElement.EnumerateObject())
            {
                JsonElement entryValue = entryProperty.Value;

                string typeName = entryValue.GetProperty(TypePropertyName).GetString();
                if (string.IsNullOrEmpty(typeName))
                {
                    table[entryProperty.Name] = null;
                    continue;
                }

                Type valueType = Type.GetType(typeName, throwOnError: true);
                JsonElement valueElement = entryValue.GetProperty(ValuePropertyName);
                object value = JsonSerializer.Deserialize(valueElement.GetRawText(), valueType, options);
                table[entryProperty.Name] = value;
            }

            return table;
        }

        public override void Write(Utf8JsonWriter writer, Hashtable value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            foreach (DictionaryEntry entry in value)
            {
                writer.WritePropertyName(entry.Key.ToString());
                WriteTypedValue(writer, entry.Value, options);
            }
            writer.WriteEndObject();
        }

        private static void WriteTypedValue(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            if (value == null)
            {
                writer.WriteString(TypePropertyName, string.Empty);
                writer.WriteNull(ValuePropertyName);
            }
            else
            {
                Type valueType = value.GetType();
                writer.WriteString(TypePropertyName, valueType.AssemblyQualifiedName);
                writer.WritePropertyName(ValuePropertyName);
                JsonSerializer.Serialize(writer, value, valueType, options);
            }
            writer.WriteEndObject();
        }
    }
}