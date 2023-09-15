using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Pattern.Synchro.Client;

public static class Serializer
{
    public static async Task<T> Deserialize<T>(Stream stream, IJsonTypeInfoResolver typeInfoResolver)
    {
        var text = await new StreamReader(stream).ReadToEndAsync();
        
        var entities = JsonSerializer.Deserialize<T>(text, new JsonSerializerOptions
        {
            TypeInfoResolver = typeInfoResolver,
            ReferenceHandler = ReferenceHandler.Preserve,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        });
        return entities;
    }
    
    public static Task<string> Serialize<T>(T synchroDevice, IJsonTypeInfoResolver typeInfoResolver)
    {
        var serialize = JsonSerializer.Serialize((object)synchroDevice, new JsonSerializerOptions
        {
            TypeInfoResolver = typeInfoResolver,
            ReferenceHandler = ReferenceHandler.Preserve,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return Task.FromResult(serialize);
    }
}