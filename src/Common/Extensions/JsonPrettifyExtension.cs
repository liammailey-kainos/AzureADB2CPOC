using System.Text.Json;

namespace Common.Extensions
{
    public static class JsonPrettifyExtension
    {
        public static string JsonPrettify(this string uglyJson)
        {
            using var jDoc = JsonDocument.Parse(uglyJson);
            return JsonSerializer.Serialize(jDoc, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
