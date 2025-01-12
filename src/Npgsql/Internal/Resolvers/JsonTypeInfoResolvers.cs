using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Npgsql.Internal.Converters;
using Npgsql.Internal.Postgres;

namespace Npgsql.Internal.Resolvers;

class JsonTypeInfoResolver : IPgTypeInfoResolver
{
    protected TypeInfoMappingCollection Mappings { get; } = new();

    public JsonTypeInfoResolver(JsonSerializerOptions? serializerOptions = null)
        => AddTypeInfos(Mappings, serializerOptions);

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Only used to request rooted and statically known types (JsonDocument,JsonElement etc).")]
    [UnconditionalSuppressMessage("Aot", "IL3050", Justification = "Only used to request rooted and statically known types  (JsonDocument,JsonElement etc).")]
    static void AddTypeInfos(TypeInfoMappingCollection mappings, JsonSerializerOptions? serializerOptions = null)
    {
#if NET7_0_OR_GREATER
        serializerOptions ??= JsonSerializerOptions.Default;
#else
        if (serializerOptions is null)
        {
            serializerOptions = new JsonSerializerOptions();
            serializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
        }
#endif

        // Jsonb is the first default for JsonDocument
        foreach (var dataTypeName in new[] { DataTypeNames.Jsonb, DataTypeNames.Json })
        {
            var jsonb = dataTypeName == DataTypeNames.Jsonb;
            mappings.AddType<JsonDocument>(dataTypeName, (options, mapping, _) =>
                    mapping.CreateInfo(options, new JsonConverter<JsonDocument, JsonDocument>(jsonb, options.TextEncoding, serializerOptions)),
                isDefault: true);
            mappings.AddStructType<JsonElement>(dataTypeName, (options, mapping, _) =>
                    mapping.CreateInfo(options, new JsonConverter<JsonElement, JsonElement>(jsonb, options.TextEncoding, serializerOptions)));
        }
    }

    protected static void AddArrayInfos(TypeInfoMappingCollection mappings)
    {
        foreach (var dataTypeName in new[] { DataTypeNames.Jsonb, DataTypeNames.Json })
        {
            mappings.AddArrayType<JsonDocument>(dataTypeName);
            mappings.AddStructArrayType<JsonElement>(dataTypeName);
        }
    }

    public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
        => Mappings.Find(type, dataTypeName, options);
}

sealed class JsonArrayTypeInfoResolver : JsonTypeInfoResolver, IPgTypeInfoResolver
{
    new TypeInfoMappingCollection Mappings { get; }

    public JsonArrayTypeInfoResolver(JsonSerializerOptions? serializerOptions = null) : base(serializerOptions)
    {
        Mappings = new TypeInfoMappingCollection(base.Mappings);
        AddArrayInfos(Mappings);
    }

    public new PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
        => Mappings.Find(type, dataTypeName, options);
}
