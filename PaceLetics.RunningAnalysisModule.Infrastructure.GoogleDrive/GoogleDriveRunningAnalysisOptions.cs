namespace PaceLetics.RunningAnalysisModule.Infrastructure.GoogleDrive;

public sealed class GoogleDriveRunningAnalysisOptions
{
    public const string SectionName = "RunningAnalysis:GoogleDrive";

    public string ApplicationName { get; set; } = "PaceLetics";
    public string RootFolderId { get; set; } = string.Empty;
    public string RootFolderName { get; set; } = "PaceLetics Laufanalysen";
    public string ServiceAccountJsonPath { get; set; } = string.Empty;
    public string ServiceAccountJson { get; set; } = string.Empty;
}
