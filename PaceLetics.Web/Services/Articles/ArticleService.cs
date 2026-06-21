namespace PaceLetics.Web.Services.Articles;

public sealed class ArticleService : IArticleService
{
    private readonly IArticleRepository _repository;

    public ArticleService(IArticleRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Article> GetArticles()
    {
        return _repository.GetArticles()
            .Where(article => !string.IsNullOrWhiteSpace(article.Id)
                && !string.IsNullOrWhiteSpace(article.Title))
            .OrderBy(article => article.SortOrder)
            .ThenBy(article => article.Title)
            .ToList();
    }

    public IReadOnlyList<ArticlePreview> GetArticlePreviews()
    {
        return GetArticles()
            .Select(ArticlePreview.FromArticle)
            .ToList();
    }

    public Article? GetArticle(string? articleId)
    {
        if (string.IsNullOrWhiteSpace(articleId))
            return null;

        return GetArticles()
            .FirstOrDefault(article => string.Equals(article.Id, articleId, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<string> GetCategories()
    {
        return GetArticlePreviews()
            .Select(article => article.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
