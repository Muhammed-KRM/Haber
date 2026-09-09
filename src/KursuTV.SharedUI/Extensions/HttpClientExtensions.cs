using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KursuTV.SharedUI.Extensions;

public static class HttpClientExtensions
{
    public static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
