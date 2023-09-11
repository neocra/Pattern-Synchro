using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Pattern.Synchro.Client;

public static class Serializer
{
    public static async Task<T> Deserialize<T>(Stream stream, IJsonTypeInfoResolver typeInfoResolver)
    {
        var text = await new StreamReader(stream).ReadToEndAsync();
        
        var entities = JsonSerializer.Deserialize<T>(text, new JsonSerializerOptions
        {
            TypeInfoResolver = typeInfoResolver,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return entities;
    }
    
    public static Task<string> Serialize<T>(T synchroDevice, IJsonTypeInfoResolver typeInfoResolver)
    {
        var serialize = JsonSerializer.Serialize((object)synchroDevice, new JsonSerializerOptions
        {
            TypeInfoResolver = typeInfoResolver,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return Task.FromResult(serialize);
    }
}
