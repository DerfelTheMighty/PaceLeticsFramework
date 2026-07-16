namespace PaceLetics.Tests;

public sealed class RunningAnalysisMediaRecorderJavaScriptTests
{
    [Fact]
    public void ZipSignaturesUsedByManualExportAreDeclared()
    {
        var source = File.ReadAllText(GetRecorderScriptPath());

        Assert.Contains("const zipLocalFileHeaderSignature = 0x04034b50;", source);
        Assert.Contains("const zipCentralDirectorySignature = 0x02014b50;", source);
        Assert.Contains("const zipEndOfCentralDirectorySignature = 0x06054b50;", source);
    }

    [Fact]
    public void ManualExportHasDownloadFallbackWhenSharingIsUnsupported()
    {
        var source = File.ReadAllText(GetRecorderScriptPath());

        Assert.Contains("downloadRecordingPackage(packageFile)", source);
        Assert.Contains("link.download = packageFile.name;", source);
        Assert.Contains("new Blob([packageFile], { type: \"application/octet-stream\" })", source);
        Assert.Contains("URL.createObjectURL(downloadBlob)", source);
    }

    private static string GetRecorderScriptPath()
    {
        var repositoryRoot = FindRepositoryRoot(new DirectoryInfo(Directory.GetCurrentDirectory()))
            ?? FindRepositoryRoot(new DirectoryInfo(AppContext.BaseDirectory))
            ?? throw new DirectoryNotFoundException("Could not locate the repository root.");

        return Path.Combine(
            repositoryRoot.FullName,
            "PaceLetics.RunningAnalysisModule.Components",
            "wwwroot",
            "runningAnalysisMediaRecorder.js");
    }

    private static DirectoryInfo? FindRepositoryRoot(DirectoryInfo? directory)
    {
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "PaceLeticsFramework.sln")))
                return directory;

            directory = directory.Parent;
        }

        return null;
    }
}
