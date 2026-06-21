using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.Web.Services.Academy;

public static class AcademyArticleCategories
{
    public const string Fundamentals = "fundamentals";
    public const string RunningAnalysis = "runningAnalysis";
}

public sealed class AcademyArticle
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Category { get; init; } = AcademyArticleCategories.Fundamentals;
    public string SourceModule { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> BodyBlocks { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ContentReference> References { get; init; } = Array.Empty<ContentReference>();
    public int SortOrder { get; init; }
}

public interface IAcademyService
{
    IReadOnlyList<AcademyArticle> GetArticles();
    IReadOnlyList<string> GetCategories();
}
