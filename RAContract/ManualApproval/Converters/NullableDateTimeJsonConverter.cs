using Newtonsoft.Json;
using System;

namespace AvePoint.RA.Contract.ManualApproval.Converters
{
    public class NullableDateTimeJsonConverter : JsonConverter<DateTime?>
    {
        public override DateTime? ReadJson(JsonReader reader, Type objectType, DateTime? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Date) return (DateTime?)reader.Value;

            if (reader.TokenType == JsonToken.String && DateTime.TryParse(reader.Value?.ToString(), out var date)) return date;

            return DateTime.UtcNow;
        }

        public override void WriteJson(JsonWriter writer, DateTime? value, JsonSerializer serializer)
        {
            if (value.HasValue) writer.WriteValue(value.Value.ToString("O"));
            else writer.WriteNull();
        }
    }
}
