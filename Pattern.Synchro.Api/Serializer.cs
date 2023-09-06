using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

#if NETSTANDARD2_0
namespace Pattern.Synchro.Client;
#else
namespace Pattern.Synchro.Api;
#endif

public class Serializer
{
    public static IJsonTypeInfoResolver TypeInfoResolver { get; set; }
    
    public static async Task<T> Deserialize<T>(Stream stream)
    {
        var text = await new StreamReader(stream).ReadToEndAsync();
        
        var entities = JsonSerializer.Deserialize<T>(text, new JsonSerializerOptions
        {
            TypeInfoResolver = TypeInfoResolver,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return entities;
    }
    
    public static Task<string> Serialize<T>(T synchroDevice)
    {
        var serialize = JsonSerializer.Serialize((object)synchroDevice, new JsonSerializerOptions
        {
            TypeInfoResolver = TypeInfoResolver,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return Task.FromResult(serialize);
    }
}