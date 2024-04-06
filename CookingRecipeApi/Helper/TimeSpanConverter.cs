using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace CookingRecipeApi.Helper
{
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(UInt32.TryParse(reader.GetString(), out uint seconds))
            {
                return TimeSpan.FromSeconds(seconds);
            }
            return TimeSpan.Zero;
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((uint)value.TotalSeconds);
        }
    }
}
