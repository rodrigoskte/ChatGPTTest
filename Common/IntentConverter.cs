using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace OpenIATest.Common
{
    public class IntentConverter : JsonConverter<List<string>>
    {
        public override void WriteJson(JsonWriter writer, List<string> value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override List<string> ReadJson(JsonReader reader, Type objectType, List<string> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type == JTokenType.String)
            {
                var stringValue = token.ToString();
                return JsonConvert.DeserializeObject<List<string>>(stringValue);
            }
            return new List<string>();
        }
    }

}
