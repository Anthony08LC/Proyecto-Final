using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuoteFlow.Api.Tests.Infrastructure;

/// <summary>Mismas convenciones JSON que la API: camelCase y enumeraciones como texto.</summary>
public static class TestJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
