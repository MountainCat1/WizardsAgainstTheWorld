using Newtonsoft.Json;
using Zenject;

namespace Utilities
{
    public interface IJsonService
    {
        string Serialize<T>(T obj);
        T Deserialize<T>(string json);
    }

    public class JsonService : IJsonService
    {
        [Inject] private JsonSerializerSettings _jsonSerializerSettings;
        
        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, _jsonSerializerSettings);
        }
        
        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);
        }
    }
}