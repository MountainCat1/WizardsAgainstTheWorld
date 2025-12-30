using System.Collections.Generic;
using Newtonsoft.Json;
using Utilities;

namespace Data
{
    public static class DataCloner
    {
        public static JsonSerializerSettings Settings => new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = new List<JsonConverter>
                { new Vector2Converter(), new Vector2IntConverter() } // Add Vector2 support
        };
        
        public static T Clone<T>(T source)
        {
            var serialized = JsonConvert.SerializeObject(source, Settings);
            return JsonConvert.DeserializeObject<T>(serialized, Settings);
        }
    }
}