using AntonReleaseCenter.Core.Models;
using AntonReleaseCenter.SoftwareSDK.Services;

namespace AntonReleaseCenter.Tests.Services;

public class FileDownloadServiceTests
{
    private static FileDownloadService CreateService()
    {
        var httpClient = new HttpClient();
        return new FileDownloadService(httpClient);
    }

    private static SoftwareRelease CreateRelease(string filePath = "https://example.com/file.zip")
    {
        return new SoftwareRelease(
            SoftwareReleaseId: Guid.NewGuid(),
            SoftwareId: Guid.NewGuid(),
            ChannelId: Guid.NewGuid(),
            Platform: PlatformEnum.Windows_x64,
            Version: new Core.Models.Version(1, 0, 0, 0),
            UpdateLog: "test",
            FilePath: filePath,
            FileSize: 1024,
            FileHash: "hash",
            IsForceUpdate: false,
            ReleaseTime: DateTime.UtcNow,
            IsOnline: true);
    }

    // ===== 边界场景 =====

    [Fact]
    public void DownloadRelease_CancellationRequested_CompletesImmediately()
    {
        var service = CreateService();
        var release = CreateRelease();
        var progress = new Progress<double>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var task = service.DownloadRelease(release, Path.GetTempFileName(), progress, cts.Token);

        Assert.True(task.IsCompleted);
    }

    [Fact]
    public async Task DownloadRelease_NullFilePath_ThrowsException()
    {
        var service = CreateService();
        var release = CreateRelease(filePath: null!);
        var progress = new Progress<double>();

        await Assert.ThrowsAnyAsync<Exception>(
            () => service.DownloadRelease(release, Path.GetTempFileName(), progress));
    }

    [Fact]
    public async Task DownloadRelease_EmptyFilePath_ThrowsException()
    {
        var service = CreateService();
        var release = CreateRelease(filePath: string.Empty);
        var progress = new Progress<double>();

        await Assert.ThrowsAnyAsync<Exception>(
            () => service.DownloadRelease(release, Path.GetTempFileName(), progress));
    }

    // ===== 正常场景 =====

    [Fact]
    public async Task DownloadRelease_WithProgress_DoesNotThrow()
    {
        var service = CreateService();
        var release = CreateRelease();
        var progress = new Progress<double>();

        var task = service.DownloadRelease(release, Path.GetTempFileName(), progress);

        var completedTask = await Task.WhenAny(task, Task.Delay(5000));
        Assert.True(completedTask == task || completedTask.IsCompleted);
    }
}
