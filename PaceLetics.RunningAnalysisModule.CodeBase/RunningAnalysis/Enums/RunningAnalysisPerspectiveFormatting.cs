namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Enums;

public static class RunningAnalysisPerspectiveFormatting
{
    public static string ToFileNameToken(RunningAnalysisPerspective perspective)
    {
        return perspective == RunningAnalysisPerspective.Rear ? "rear" : "side";
    }

    public static string ToDisplayName(RunningAnalysisPerspective perspective)
    {
        return perspective == RunningAnalysisPerspective.Rear ? "Hinten" : "Seite";
    }

    public static RunningAnalysisPerspective ParseOrDefault(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        return normalized switch
        {
            "rear" or "back" or "hinten" or "rueck" or "rück" => RunningAnalysisPerspective.Rear,
            _ => RunningAnalysisPerspective.Side
        };
    }
}

