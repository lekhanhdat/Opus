using Newtonsoft.Json;
using System;

namespace AvePoint.RA.Api.Contract
{
    public class StrictBooleanJsonConverter : JsonConverter<bool>
    {
        public override bool ReadJson(JsonReader reader, Type objectType, bool existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Boolean)
            {
                throw new JsonSerializationException($"Property '{reader.Path}' must be a boolean value.");
            }

            return (bool)reader.Value;
        }

        public override void WriteJson(JsonWriter writer, bool value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }
}