namespace PaceLetics.RunningAnalysisModule.Infrastructure.GoogleDrive;

public sealed class GoogleDriveRunningAnalysisOptions
{
    public const string SectionName = "PaceLeticsUserData:GoogleDrive";
    public const string FlatSectionName = "PaceLeticsUserData";
    public const string LegacySectionName = "RunningAnalysis:GoogleDrive";

    public string ApplicationName { get; set; } = "PaceLetics";
    public string RootFolderId { get; set; } = string.Empty;
    public string RootFolderName { get; set; } = "paceletics_user_data";
    public string ServiceAccountJsonPath { get; set; } = string.Empty;
    public string ServiceAccountJson { get; set; } = string.Empty;
}
