using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LuckyDrawBot.Infrastructure
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString(), null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffZ", CultureInfo.InvariantCulture));
        }
    }
}
