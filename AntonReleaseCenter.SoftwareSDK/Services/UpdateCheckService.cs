using System.Net;
using System.Text.Json;
using AntonReleaseCenter.Core.DTOs;
using AntonReleaseCenter.SoftwareSDK.Model;

namespace AntonReleaseCenter.SoftwareSDK.Services;

public sealed class UpdateCheckService(HttpClient httpClient, Configure configure)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly Configure _configure = configure;

    public async Task<CheckUpdateResponse?> CheckUpdateAsync(
        string currentVersion,
        int channelCode = 0,
        string? deviceId = null,
        CancellationToken ct = default)
    {
        var effectiveChannelCode = channelCode != 0 ? channelCode : _configure.ChannelCode;

        var queryParameters = new List<string>
        {
            $"appKey={WebUtility.UrlEncode(_configure.SoftwareKey)}",
            $"channelCode={effectiveChannelCode}",
            $"currentVersion={WebUtility.UrlEncode(currentVersion)}"
        };

        if (!string.IsNullOrEmpty(deviceId))
            queryParameters.Add($"deviceId={WebUtility.UrlEncode(deviceId)}");

        var url = $"{_configure.Url}/public/check-update?{string.Join("&", queryParameters)}";

        var result = await _httpClient.GetAsync(url, ct);
        if (result.StatusCode == HttpStatusCode.NotFound)
            return null;

        result.EnsureSuccessStatusCode();

        var json = await result.Content.ReadAsStringAsync(ct);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<CheckUpdateResponse>(json, options);
    }
}
