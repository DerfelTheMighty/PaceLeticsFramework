using System.Text.Json;
using Microsoft.JSInterop;

namespace PaceLetics.Web.Services;

public sealed class ClientPreferenceStore
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    private readonly IJSRuntime _js;

    public ClientPreferenceStore(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var json = await _js.InvokeAsync<string?>("paceleticsStorage.get", key);
            return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, Options);
        }
        catch (InvalidOperationException)
        {
            return default;
        }
        catch (JSException)
        {
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value)
    {
        try
        {
            await _js.InvokeVoidAsync("paceleticsStorage.set", key, JsonSerializer.Serialize(value, Options));
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSException)
        {
        }
    }
}
