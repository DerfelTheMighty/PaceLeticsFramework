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
    public string DelegatedUserEmail { get; set; } = string.Empty;
    public string OAuthClientId { get; set; } = string.Empty;
    public string OAuthClientSecret { get; set; } = string.Empty;
    public string OAuthRefreshToken { get; set; } = string.Empty;
    public string OAuthUserEmail { get; set; } = string.Empty;
    public bool EnablePublicReadLinks { get; set; }

    public bool HasValidCredentialShape()
    {
        var oauthValues = new[] { OAuthClientId, OAuthClientSecret, OAuthRefreshToken };
        var configuredOauthValues = oauthValues.Count(value => !string.IsNullOrWhiteSpace(value));
        return configuredOauthValues is 0 or 3;
    }
}
