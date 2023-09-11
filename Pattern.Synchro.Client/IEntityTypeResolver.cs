using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Pattern.Synchro.Client;

public class EntityTypeResolver : DefaultJsonTypeInfoResolver
{
    private readonly Type[] types;

    public EntityTypeResolver(Type[] types)
    {
        this.types = types;
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        var basePointType = typeof(IEntity);
        if (jsonTypeInfo.Type == basePointType)
        {
            var jsonPolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "typename",
                IgnoreUnrecognizedTypeDiscriminators = false,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
            };

            foreach (var derivedType in this.types)
            {
                var assemblyFullName = derivedType.Assembly.FullName.Split(',')[0];

                jsonPolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType, $"{derivedType.FullName}, {assemblyFullName}"));
            }

            jsonTypeInfo.PolymorphismOptions = jsonPolymorphismOptions;
        }

        return jsonTypeInfo;
    }
}