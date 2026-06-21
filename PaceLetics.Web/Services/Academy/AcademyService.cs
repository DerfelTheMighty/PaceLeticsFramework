using Microsoft.Extensions.Localization;
using PaceLetics.RunningAnalysisModule.Components;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;
using PaceLetics.Web.Pages.Athletes;
using AcademyPage = PaceLetics.Web.Pages.Academy.Academy;

namespace PaceLetics.Web.Services.Academy;

public sealed partial class AcademyService : IAcademyService
{
    private readonly IStringLocalizer<AcademyPage> _academyLocalizer;
    private readonly IStringLocalizer<Dashboard> _dashboardLocalizer;
    private readonly IStringLocalizer<RunningAnalysisResources> _runningAnalysisLocalizer;

    public AcademyService(
        IStringLocalizer<AcademyPage> academyLocalizer,
        IStringLocalizer<Dashboard> dashboardLocalizer,
        IStringLocalizer<RunningAnalysisResources> runningAnalysisLocalizer)
    {
        _academyLocalizer = academyLocalizer;
        _dashboardLocalizer = dashboardLocalizer;
        _runningAnalysisLocalizer = runningAnalysisLocalizer;
    }

    public IReadOnlyList<AcademyArticle> GetArticles()
    {
        return BuildArticles()
            .Where(article => !string.IsNullOrWhiteSpace(article.Title))
            .OrderBy(article => article.SortOrder)
            .ThenBy(article => article.Title)
            .ToList();
    }

    public IReadOnlyList<string> GetCategories()
    {
        return GetArticles()
            .Select(article => article.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private IEnumerable<AcademyArticle> BuildArticles()
    {
        foreach (var article in BuildMentalResourceArticles())
            yield return article;

        foreach (var article in BuildRunningAnalysisArticles())
            yield return article;

        foreach (var article in BuildPaceControlledTrainingArticles())
            yield return article;
    }

    private IEnumerable<AcademyArticle> BuildMentalResourceArticles()
    {
        var references = BuildMentalResourceReferences();
        var lead = DashboardText("MentalResource_Lead", "Running can become a psychological resource when load, recovery, and routine fit together.");

        yield return new AcademyArticle
        {
            Id = "mental-resource-running",
            Title = AcademyText("ArticleMentalResourceTitle", "Laufen als mentale Resource"),
            Summary = lead,
            Category = AcademyArticleCategories.Fundamentals,
            SourceModule = "Academy mental resource",
            Tags = new[] { "Mental resource", "Sustainability", "Injury prevention", "Beginners", "Drop-out" },
            BodyBlocks = Blocks(
                lead,
                DashboardText("MentalResource_StepEffectText", "Regular recreational running can support mood and mental health."),
                DashboardText("MentalResource_StepFragileText", "Running as a resource depends on load tolerance, recovery, and everyday constraints."),
                DashboardText("MentalResource_StepRiskText", "Novice runners show higher reported injury incidence and early drop-out in beginner programs."),
                DashboardText("MentalResource_StepConsequenceText", "Sustainable programs cannot treat drop-out as a side issue."),
                DashboardText("MentalResource_StepAppText", "PaceLetics doses load and makes routines understandable.")),
            References = references,
            SortOrder = 10
        };
    }

    private IEnumerable<AcademyArticle> BuildRunningAnalysisArticles()
    {
        var references = BuildRunningAnalysisReferences();

        yield return new AcademyArticle
        {
            Id = "evidence-based-running-analysis",
            Title = AcademyText("ArticleRunningAnalysisTitle", "Evidenzbasierte Laufanalyse"),
            Summary = AcademyText("ArticleRunningAnalysisSummary", "Warum Laufanalyse Evidenz, Kontext und gemeinsame Entscheidungen braucht."),
            Category = AcademyArticleCategories.RunningAnalysis,
            SourceModule = "Running analysis guidance",
            Tags = new[] { "Running analysis", "Evidence", "Shared decision", "Do no harm" },
            BodyBlocks = Blocks(
                RunningAnalysisText("InterventionGuidance_CompactIntro", "Running style is a self-optimizing system."),
                RunningAnalysisText("InterventionGuidance_SystemUnderstandingText", "Bodies differ and running style is individual."),
                RunningAnalysisText("InterventionGuidance_FunctioningSystemAlert", "A long symptom-free pattern can be a functioning dynamic system."),
                RunningAnalysisText("InterventionGuidance_SideEffectsIntro", "Technique changes rarely just reduce load; they shift it."),
                RunningAnalysisText("InterventionGuidance_SideEffectsIndicationIntro", "Interventions need an indication."),
                RunningAnalysisText("InterventionGuidance_SideEffectsIndication_1", "Moderate to good evidence can justify an intervention."),
                RunningAnalysisText("InterventionGuidance_SideEffectsIndication_2", "Symptoms and plausible links matter when evidence is weaker."),
                RunningAnalysisText("InterventionGuidance_SideEffectsConclusion", "Most common criteria of good running form do not meet these requirements."),
                RunningAnalysisText("InterventionGuidance_ConsentPowerDynamicsText", "Running-style interventions require informed consent and context."),
                RunningAnalysisText("InterventionGuidance_Talk_1", "A note about running style is not a judgment."),
                RunningAnalysisText("InterventionGuidance_Talk_3", "Running style belongs in the context of symptoms, training, shoes, pace, volume, and fatigue.")),
            References = references,
            SortOrder = 20
        };
    }

    private IEnumerable<AcademyArticle> BuildPaceControlledTrainingArticles()
    {
        yield return new AcademyArticle
        {
            Id = "pace-controlled-training",
            Title = AcademyText("ArticlePaceTrainingTitle", "Pacegesteuertes Training"),
            Summary = AcademyText("ArticlePaceTrainingSummary", "Warum Pacebereiche Trainingsbelastung steuerbar und nachvollziehbar machen."),
            Category = AcademyArticleCategories.Training,
            SourceModule = "PaceLetics training system",
            Tags = new[] { "Pace", "VDOT", "Critical speed", "Training load" },
            BodyBlocks = Blocks(
                AcademyText("ArticlePaceTrainingBody1", "Pacegesteuertes Training übersetzt Leistungsdaten in konkrete Geschwindigkeitsbereiche. Dadurch wird aus einem Plan nicht nur eine Strecke, sondern eine steuerbare Belastung."),
                AcademyText("ArticlePaceTrainingBody2", "Pacebereiche helfen, lockere Läufe wirklich locker zu halten und harte Einheiten gezielt zu dosieren. Das schützt vor dem typischen Muster, jede Einheit unbewusst mittelhart zu laufen."),
                AcademyText("ArticlePaceTrainingBody3", "PaceLetics nutzt Referenzleistungen, VDOT und Critical Speed, um Training verständlich zu machen. Die Pace ist dabei kein Selbstzweck, sondern ein Werkzeug zur Orientierung."),
                AcademyText("ArticlePaceTrainingBody4", "Wenn Form, Müdigkeit, Wetter oder Gelände nicht passen, bleibt Wahrnehmung wichtig. Gute Trainingssteuerung verbindet Zahlen mit Körpergefühl.")),
            References = Array.Empty<ContentReference>(),
            SortOrder = 30
        };
    }

    private List<ContentReference> BuildMentalResourceReferences()
    {
        return new List<ContentReference>
        {
            Reference("MentalResource_SourceOswald", "https://doi.org/10.3390/ijerph17218059", _dashboardLocalizer),
            Reference("MentalResource_SourcePereira", "https://doi.org/10.3389/fpsyg.2021.624783", _dashboardLocalizer),
            Reference("MentalResource_SourceFurie", "https://doi.org/10.1007/s12178-023-09830-6", _dashboardLocalizer),
            Reference("MentalResource_SourceDeJonge", "https://doi.org/10.3390/ijerph17031044", _dashboardLocalizer),
            Reference("MentalResource_SourceVerhagen", "https://doi.org/10.1136/bmjsem-2021-001117", _dashboardLocalizer),
            Reference("MentalResource_SourceVidebaek", "https://doi.org/10.1007/s40279-015-0333-8", _dashboardLocalizer),
            Reference("MentalResource_SourceFokkema", "https://doi.org/10.1016/j.jsams.2018.06.003", _dashboardLocalizer),
            Reference("MentalResource_SourceRelph", "https://doi.org/10.3390/ijerph20176682", _dashboardLocalizer)
        };
    }

    private List<ContentReference> BuildRunningAnalysisReferences()
    {
        return new List<ContentReference>
        {
            Reference("InterventionGuidance_SourceCeyssens", "https://pubmed.ncbi.nlm.nih.gov/31028658/", _runningAnalysisLocalizer, "1"),
            Reference("InterventionGuidance_SourceWillwacher", "https://pubmed.ncbi.nlm.nih.gov/35247202/", _runningAnalysisLocalizer, "2"),
            Reference("InterventionGuidance_SourceAndersonStepRate", "https://link.springer.com/article/10.1186/s40798-022-00504-0", _runningAnalysisLocalizer, "3"),
            Reference("InterventionGuidance_SourceMoore", "https://link.springer.com/article/10.1007/s40279-016-0474-4", _runningAnalysisLocalizer, "4"),
            Reference("InterventionGuidance_SourceNigg", "https://bjsm.bmj.com/content/49/20/1290", _runningAnalysisLocalizer, "5"),
            Reference("InterventionGuidance_SourceGaitRetraining", "https://pmc.ncbi.nlm.nih.gov/articles/PMC9655004/", _runningAnalysisLocalizer, "6"),
            Reference("InterventionGuidance_SourceRidge", "https://pubmed.ncbi.nlm.nih.gov/23439417/", _runningAnalysisLocalizer, "7"),
            Reference("InterventionGuidance_SourceChristopher", "https://pubmed.ncbi.nlm.nih.gov/30805204/", _runningAnalysisLocalizer, "8")
        };
    }

    private static IReadOnlyList<ContentReference> ReferencesByNumber(
        IReadOnlyList<ContentReference> references,
        params string[] numbers)
    {
        var numberSet = numbers.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return references
            .Where(reference => numberSet.Contains(reference.SourceType))
            .ToList();
    }

    private static ContentReference Reference<T>(
        string titleKey,
        string url,
        IStringLocalizer<T> localizer,
        string sourceType = "study")
    {
        return new ContentReference
        {
            Title = Localized(localizer, titleKey, titleKey),
            Url = url,
            SourceType = sourceType
        };
    }

    private string DashboardText(string key, string fallback)
    {
        return Localized(_dashboardLocalizer, key, fallback);
    }

    private string AcademyText(string key, string fallback)
    {
        return Localized(_academyLocalizer, key, fallback);
    }

    private string RunningAnalysisText(string key, string fallback)
    {
        return Localized(_runningAnalysisLocalizer, key, fallback);
    }

    private static string Localized<T>(IStringLocalizer<T> localizer, string key, string fallback)
    {
        var localized = localizer[key];
        return localized.ResourceNotFound ? fallback : localized.Value;
    }

    private static IReadOnlyList<string> Blocks(params string[] values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToList();
    }

}
