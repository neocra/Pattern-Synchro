using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Pattern.Synchro;

public class Serializer
{
    public static async Task<T> Deserialize<T>(Stream stream)
    {
        var streamReader = new StreamReader(stream);

        var entities = JsonConvert.DeserializeObject<T>(
            await streamReader.ReadToEndAsync().ConfigureAwait(false),
            new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });
        return entities;
    }
    
    public static async Task<string> Serialize<T>(T synchroDevice)
    {
        return JsonConvert.SerializeObject(synchroDevice,
            new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });
    }
}