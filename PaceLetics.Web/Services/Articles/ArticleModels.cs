using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.Web.Services.Articles;

public static class ArticleCategories
{
    public const string Fundamentals = "fundamentals";
    public const string RunningAnalysis = "runningAnalysis";
    public const string Training = "training";
}

public enum ArticleContentKind
{
    Generic,
    MentalResource,
    RunningAnalysisGuidance,
    PaceModelInfo
}

public sealed class Article
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Category { get; init; } = ArticleCategories.Fundamentals;
    public string SourceModule { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> BodyBlocks { get; init; } = Array.Empty<string>();
    public string BodyHtml { get; init; } = string.Empty;
    public IReadOnlyList<ContentReference> References { get; init; } = Array.Empty<ContentReference>();
    public ArticleContentKind ContentKind { get; init; } = ArticleContentKind.Generic;
    public int SortOrder { get; init; }
}

public sealed class ArticlePreview
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Category { get; init; } = ArticleCategories.Fundamentals;
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public ArticleContentKind ContentKind { get; init; } = ArticleContentKind.Generic;
    public int SortOrder { get; init; }

    public static ArticlePreview FromArticle(Article article)
    {
        return new ArticlePreview
        {
            Id = article.Id,
            Title = article.Title,
            Summary = article.Summary,
            Category = article.Category,
            Tags = article.Tags,
            ContentKind = article.ContentKind,
            SortOrder = article.SortOrder
        };
    }
}

public interface IArticleRepository
{
    IReadOnlyList<Article> GetArticles();
}

public interface IArticleService
{
    IReadOnlyList<Article> GetArticles();
    IReadOnlyList<ArticlePreview> GetArticlePreviews();
    Article? GetArticle(string? articleId);
    IReadOnlyList<string> GetCategories();
}
