using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SkillSwapAPI.LoadTests;

public sealed class ApiClient : IDisposable
{
    private readonly HttpClient _client;

    public ApiClient(string baseUrl)
    {
        _client = new HttpClient(new SocketsHttpHandler
        {
            MaxConnectionsPerServer = 4096,
            PooledConnectionLifetime = TimeSpan.FromMinutes(10)
        })
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromMinutes(5)
        };
    }

    public HttpClient Client => _client;

    public async Task<(int Status, string Content)> SendAsync(
        HttpRequestMessage message, string? token = null)
    {
        if (token is not null)
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var response = await _client.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();
        return ((int)response.StatusCode, content);
    }

    public async Task<(int Status, string Content)> PostJsonAsync(
        string path, object body, string? token = null)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json")
        };
        return await SendAsync(message, token);
    }

    public async Task<(int Status, string Content)> PutJsonAsync(
        string path, object? body, string? token = null)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, path);
        if (body is not null)
        {
            message.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");
        }
        return await SendAsync(message, token);
    }

    public async Task<(int Status, string Content)> GetAsync(string path, string? token = null)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, path);
        return await SendAsync(message, token);
    }

    public void Dispose() => _client.Dispose();
}
