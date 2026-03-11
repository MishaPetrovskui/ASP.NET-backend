namespace UsersAPI
{
    public class DateOnlyJsonConverter : System.Text.Json.Serialization.JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref System.Text.Json.Utf8JsonReader reader,
            Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
            => DateOnly.Parse(reader.GetString()!);

        public override void Write(System.Text.Json.Utf8JsonWriter writer,
            DateOnly value, System.Text.Json.JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }
}
