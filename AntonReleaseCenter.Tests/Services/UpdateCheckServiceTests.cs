using System.Net;
using System.Text.Json;
using AntonReleaseCenter.SoftwareSDK.Model;
using AntonReleaseCenter.SoftwareSDK.Services;

namespace AntonReleaseCenter.Tests.Services;

public class UpdateCheckServiceTests
{
    private static readonly Configure DefaultConfigure = new()
    {
        Url = "https://example.com",
        SoftwareKey = "test-app",
        ChannelCode = 1,
        Platform = PlatformEnum.Windows_x64
    };

    private static UpdateCheckService CreateService(HttpMessageHandler handler, Configure? configure = null)
    {
        var httpClient = new HttpClient(handler);
        return new UpdateCheckService(httpClient, configure ?? DefaultConfigure);
    }

    private static MockHttpMessageHandler CreateHandler(
        HttpStatusCode statusCode,
        object? responseBody = null)
    {
        var json = responseBody is not null
            ? JsonSerializer.Serialize(responseBody)
            : string.Empty;
        return new MockHttpMessageHandler(statusCode, json);
    }

    // ===== 正常场景 =====

    [Fact]
    public async Task CheckUpdateAsync_HasUpdate_ReturnsResponse()
    {
        var response = new
        {
            hasUpdate = true,
            latestVersion = new { major = 2, minor = 0, build = 0, revision = 0 },
            updateLog = "New features",
            downloadUrl = "https://example.com/v2.zip",
            fileSize = 2048,
            fileHash = "abc123",
            isForceUpdate = false
        };
        var handler = CreateHandler(HttpStatusCode.OK, response);
        var service = CreateService(handler);

        var result = await service.CheckUpdateAsync("1.0.0.0");

        Assert.NotNull(result);
        Assert.True(result.HasUpdate);
        Assert.Equal("New features", result.UpdateLog);
        Assert.Equal("https://example.com/v2.zip", result.DownloadUrl);
    }

    [Fact]
    public async Task CheckUpdateAsync_NoUpdate_ReturnsResponse()
    {
        var response = new
        {
            hasUpdate = false,
            latestVersion = (object?)null,
            updateLog = (string?)null,
            downloadUrl = (string?)null,
            fileSize = (int?)null,
            fileHash = (string?)null,
            isForceUpdate = false
        };
        var handler = CreateHandler(HttpStatusCode.OK, response);
        var service = CreateService(handler);

        var result = await service.CheckUpdateAsync("2.0.0.0");

        Assert.NotNull(result);
        Assert.False(result.HasUpdate);
    }

    // ===== 边界场景 =====

    [Fact]
    public async Task CheckUpdateAsync_NotFound_ReturnsNull()
    {
        var handler = CreateHandler(HttpStatusCode.NotFound);
        var service = CreateService(handler);

        var result = await service.CheckUpdateAsync("1.0.0.0");

        Assert.Null(result);
    }

    [Fact]
    public async Task CheckUpdateAsync_WithCustomParameters_OverridesConfigure()
    {
        var response = new
        {
            hasUpdate = false,
            latestVersion = (object?)null,
            updateLog = (string?)null,
            downloadUrl = (string?)null,
            fileSize = (int?)null,
            fileHash = (string?)null,
            isForceUpdate = false
        };
        var handler = CreateHandler(HttpStatusCode.OK, response);
        var service = CreateService(handler);

        var result = await service.CheckUpdateAsync(
            "1.0.0.0",
            channelCode: 5,
            platform: PlatformEnum.MacOS_AppleSilicon,
            deviceId: "device-123");

        Assert.NotNull(result);
        Assert.Contains("channelCode=5", handler.LastRequestUrl);
        Assert.Contains("platform=MacOS_AppleSilicon", handler.LastRequestUrl);
        Assert.Contains("deviceId=device-123", handler.LastRequestUrl);
    }

    [Fact]
    public async Task CheckUpdateAsync_WithoutDeviceId_ExcludesDeviceIdFromUrl()
    {
        var response = new
        {
            hasUpdate = false,
            latestVersion = (object?)null,
            updateLog = (string?)null,
            downloadUrl = (string?)null,
            fileSize = (int?)null,
            fileHash = (string?)null,
            isForceUpdate = false
        };
        var handler = CreateHandler(HttpStatusCode.OK, response);
        var service = CreateService(handler);

        await service.CheckUpdateAsync("1.0.0.0");

        Assert.DoesNotContain("deviceId", handler.LastRequestUrl);
    }

    [Fact]
    public async Task CheckUpdateAsync_EmptyDeviceId_ExcludesDeviceIdFromUrl()
    {
        var response = new
        {
            hasUpdate = false,
            latestVersion = (object?)null,
            updateLog = (string?)null,
            downloadUrl = (string?)null,
            fileSize = (int?)null,
            fileHash = (string?)null,
            isForceUpdate = false
        };
        var handler = CreateHandler(HttpStatusCode.OK, response);
        var service = CreateService(handler);

        await service.CheckUpdateAsync("1.0.0.0", deviceId: "");

        Assert.DoesNotContain("deviceId", handler.LastRequestUrl);
    }

    [Fact]
    public async Task CheckUpdateAsync_UseDefaultChannelCode_WhenZero()
    {
        var response = new
        {
            hasUpdate = false,
            latestVersion = (object?)null,
            updateLog = (string?)null,
            downloadUrl = (string?)null,
            fileSize = (int?)null,
            fileHash = (string?)null,
            isForceUpdate = false
        };
        var handler = CreateHandler(HttpStatusCode.OK, response);
        var service = CreateService(handler);

        await service.CheckUpdateAsync("1.0.0.0", channelCode: 0);

        Assert.Contains("channelCode=1", handler.LastRequestUrl);
    }

    [Fact]
    public async Task CheckUpdateAsync_UseDefaultPlatform_WhenNull()
    {
        var response = new
        {
            hasUpdate = false,
            latestVersion = (object?)null,
            updateLog = (string?)null,
            downloadUrl = (string?)null,
            fileSize = (int?)null,
            fileHash = (string?)null,
            isForceUpdate = false
        };
        var handler = CreateHandler(HttpStatusCode.OK, response);
        var service = CreateService(handler);

        await service.CheckUpdateAsync("1.0.0.0", platform: null);

        Assert.Contains("platform=Windows_x64", handler.LastRequestUrl);
    }

    // ===== 异常场景 =====

    [Fact]
    public async Task CheckUpdateAsync_ServerError_ThrowsHttpRequestException()
    {
        var handler = CreateHandler(HttpStatusCode.InternalServerError);
        var service = CreateService(handler);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.CheckUpdateAsync("1.0.0.0"));
    }

    [Fact]
    public async Task CheckUpdateAsync_Unauthorized_ThrowsHttpRequestException()
    {
        var handler = CreateHandler(HttpStatusCode.Unauthorized);
        var service = CreateService(handler);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.CheckUpdateAsync("1.0.0.0"));
    }

    [Fact]
    public async Task CheckUpdateAsync_CancellationRequested_ThrowsOperationCanceled()
    {
        var response = new
        {
            hasUpdate = false,
            latestVersion = (object?)null,
            updateLog = (string?)null,
            downloadUrl = (string?)null,
            fileSize = (int?)null,
            fileHash = (string?)null,
            isForceUpdate = false
        };
        var handler = CreateHandler(HttpStatusCode.OK, response);
        var service = CreateService(handler);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.CheckUpdateAsync("1.0.0.0", ct: cts.Token));
    }
}

internal class MockHttpMessageHandler(HttpStatusCode statusCode, string responseBody) : HttpMessageHandler
{
    public string LastRequestUrl { get; private set; } = string.Empty;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        LastRequestUrl = request.RequestUri?.ToString() ?? string.Empty;
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Delay(10, cancellationToken);
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseBody)
        };
    }
}
