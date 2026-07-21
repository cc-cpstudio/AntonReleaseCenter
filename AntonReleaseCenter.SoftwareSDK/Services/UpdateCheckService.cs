using System.Text.Json;
using AntonReleaseCenter.Core.Models;
using AntonReleaseCenter.SoftwareSDK.Model;

namespace AntonReleaseCenter.SoftwareSDK.Services;

public sealed class UpdateCheckService(HttpClient httpClient, Configure configure)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly Configure _configure = configure;

    public async Task<SoftwareRelease?> GetLatestRelease()
    {
        var result = await _httpClient.GetAsync($"{_configure.Url}/software/{_configure.SoftwareId}/releases/{_configure.Platform}");
        result.EnsureSuccessStatusCode();
        var json = await result.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var release = JsonSerializer.Deserialize<SoftwareRelease>(json, options);
        return release;
    }
}