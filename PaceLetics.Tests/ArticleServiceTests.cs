using System.Globalization;
using PaceLetics.Web.Services.Articles;

namespace PaceLetics.Tests;

public sealed class ArticleServiceTests
{
    [Fact]
    public void GetArticles_ReturnsTheThreeGlobalArticlesFromMarkdown()
    {
        var service = CreateService("de");

        var articles = service.GetArticles();

        Assert.Collection(
            articles,
            article => Assert.Equal("mental-resource-running", article.Id),
            article => Assert.Equal("evidence-based-running-analysis", article.Id),
            article => Assert.Equal("pace-controlled-training", article.Id));

        var mentalResource = Assert.Single(articles, article => article.Id == "mental-resource-running");
        Assert.Equal(ArticleCategories.Fundamentals, mentalResource.Category);
        Assert.Equal(ArticleContentKind.MentalResource, mentalResource.ContentKind);
        Assert.Equal("Laufen als mentale Ressource", mentalResource.Title);
        Assert.Equal("Markdown", mentalResource.SourceModule);
        Assert.Contains("niedrigschwellige mentale Ressource", mentalResource.BodyHtml);
        Assert.Contains(mentalResource.BodyBlocks, block => block.Contains("Einsteiger:innen", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(mentalResource.References, reference => reference.Url.Contains("ijerph17218059"));
        Assert.Contains(mentalResource.References, reference => reference.Url.Contains("CD004366.pub7"));
        Assert.Contains(mentalResource.References, reference => reference.Url.Contains("jsams.2018.06.003"));

        var runningAnalysis = Assert.Single(articles, article => article.Id == "evidence-based-running-analysis");
        Assert.Equal(ArticleCategories.RunningAnalysis, runningAnalysis.Category);
        Assert.Equal(ArticleContentKind.RunningAnalysisGuidance, runningAnalysis.ContentKind);
        Assert.Equal("Evidenzbasierte Laufanalyse", runningAnalysis.Title);
        Assert.Contains("Technikaenderungen reduzieren Belastung selten einfach", runningAnalysis.BodyHtml);
        Assert.Contains(runningAnalysis.References, reference => reference.Url.Contains("31028658"));

        var paceTraining = Assert.Single(articles, article => article.Id == "pace-controlled-training");
        Assert.Equal(ArticleCategories.Training, paceTraining.Category);
        Assert.Equal(ArticleContentKind.PaceModelInfo, paceTraining.ContentKind);
        Assert.Equal("Pacegesteuertes Training", paceTraining.Title);
        Assert.Equal("Warum wir pace-gesteuert trainieren", paceTraining.Summary);
        Assert.Contains("Critical Speed", paceTraining.BodyHtml);
        Assert.Contains(paceTraining.References, reference => reference.Url.Contains("s40279-026-02410-x"));
        Assert.Contains(paceTraining.References, reference => reference.Url.Contains("11933073"));
    }

    [Fact]
    public void GetArticles_FallsBackToEnglishWhenCultureIsMissing()
    {
        var service = CreateService("fr");

        var article = service.GetArticle("mental-resource-running");

        Assert.NotNull(article);
        Assert.Equal("Running as a mental resource", article.Title);
        Assert.Contains("low-threshold mental resource", article.BodyHtml);
    }

    [Fact]
    public void GetArticlePreviews_ReturnsLightweightCardsForAllArticles()
    {
        var service = CreateService("de");

        var previews = service.GetArticlePreviews();

        Assert.Collection(
            previews,
            preview =>
            {
                Assert.Equal("mental-resource-running", preview.Id);
                Assert.Equal("Laufen als mentale Ressource", preview.Title);
                Assert.Equal(ArticleContentKind.MentalResource, preview.ContentKind);
            },
            preview =>
            {
                Assert.Equal("evidence-based-running-analysis", preview.Id);
                Assert.Equal("Evidenzbasierte Laufanalyse", preview.Title);
                Assert.Equal(ArticleContentKind.RunningAnalysisGuidance, preview.ContentKind);
            },
            preview =>
            {
                Assert.Equal("pace-controlled-training", preview.Id);
                Assert.Equal("Pacegesteuertes Training", preview.Title);
                Assert.Equal(ArticleContentKind.PaceModelInfo, preview.ContentKind);
            });

        Assert.All(previews, preview => Assert.NotNull(preview.Tags));
    }

    [Fact]
    public void GetArticle_ReturnsArticleByIdCaseInsensitively()
    {
        var service = CreateService("de");

        var article = service.GetArticle("PACE-CONTROLLED-TRAINING");

        Assert.NotNull(article);
        Assert.Equal("pace-controlled-training", article.Id);
    }

    [Fact]
    public void GetCategories_ReturnsCategoriesWithSeedArticles()
    {
        var service = CreateService("de");

        var categories = service.GetCategories();

        Assert.Contains(ArticleCategories.Fundamentals, categories);
        Assert.Contains(ArticleCategories.RunningAnalysis, categories);
        Assert.Contains(ArticleCategories.Training, categories);
        Assert.DoesNotContain("workouts", categories);
        Assert.DoesNotContain("trainingPlans", categories);
    }

    private static ArticleService CreateService(string cultureName)
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
        var repository = new MarkdownArticleRepository(FindWebProjectRoot());
        return new ArticleService(repository);
    }

    private static string FindWebProjectRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "PaceLetics.Web");
            if (Directory.Exists(Path.Combine(candidate, "Content", "Academy")))
                return candidate;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find PaceLetics.Web/Content/Academy.");
    }
}
