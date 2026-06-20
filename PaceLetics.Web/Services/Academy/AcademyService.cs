using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using PaceLetics.RunningAnalysisModule.Components;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;
using PaceLetics.TrainingPlanModule.CodeBase.Models;
using PaceLetics.Web.Pages.Athletes;

namespace PaceLetics.Web.Services.Academy;

public sealed partial class AcademyService : IAcademyService
{
    private readonly IStringLocalizer<Dashboard> _dashboardLocalizer;
    private readonly IStringLocalizer<RunningAnalysisResources> _runningAnalysisLocalizer;
    private readonly ITrainingPlanService _trainingPlanService;

    public AcademyService(
        IStringLocalizer<Dashboard> dashboardLocalizer,
        IStringLocalizer<RunningAnalysisResources> runningAnalysisLocalizer,
        ITrainingPlanService trainingPlanService)
    {
        _dashboardLocalizer = dashboardLocalizer;
        _runningAnalysisLocalizer = runningAnalysisLocalizer;
        _trainingPlanService = trainingPlanService;
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

        foreach (var article in BuildTrainingPlanArticles())
            yield return article;
    }

    private IEnumerable<AcademyArticle> BuildMentalResourceArticles()
    {
        var references = BuildMentalResourceReferences();
        var lead = DashboardText("MentalResource_Lead", "Running can become a psychological resource when load, recovery, and routine fit together.");

        yield return new AcademyArticle
        {
            Id = "mental-resource-running",
            Title = DashboardText("MentalResource_Title", "Why running is more than training"),
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
        var commonTags = new[] { "Running analysis", "Shared decision", "Do no harm" };

        yield return new AcademyArticle
        {
            Id = "running-analysis-intervention-philosophy",
            Title = RunningAnalysisText("InterventionGuidance_Title", "Intervention philosophy"),
            Summary = RunningAnalysisText("InterventionGuidance_Subtitle", "No intervention without indication."),
            Category = AcademyArticleCategories.RunningAnalysis,
            SourceModule = "Running analysis guidance",
            Tags = commonTags,
            BodyBlocks = Blocks(
                RunningAnalysisText("InterventionGuidance_CompactIntro", "Running style is a self-optimizing system."),
                RunningAnalysisText("InterventionGuidance_SystemUnderstandingText", "Bodies differ and running style is individual."),
                RunningAnalysisText("InterventionGuidance_FunctioningSystemAlert", "A long symptom-free pattern can be a functioning dynamic system.")),
            References = ReferencesByNumber(references, "4"),
            SortOrder = 30
        };

        yield return new AcademyArticle
        {
            Id = "running-analysis-side-effects",
            Title = RunningAnalysisText("InterventionGuidance_SideEffectsTitle", "Interventions have side effects"),
            Summary = RunningAnalysisText("InterventionGuidance_SideEffectsIntro", "Technique changes rarely just reduce load; they shift it."),
            Category = AcademyArticleCategories.RunningAnalysis,
            SourceModule = "Running analysis guidance",
            Tags = commonTags,
            BodyBlocks = Blocks(
                RunningAnalysisText("InterventionGuidance_SideEffectsIntro", "Technique changes rarely just reduce load; they shift it."),
                RunningAnalysisText("InterventionGuidance_SideEffectsIndicationIntro", "Interventions need an indication."),
                RunningAnalysisText("InterventionGuidance_SideEffectsIndication_1", "Moderate to good evidence can justify an intervention."),
                RunningAnalysisText("InterventionGuidance_SideEffectsIndication_2", "Symptoms and plausible links matter when evidence is weaker."),
                RunningAnalysisText("InterventionGuidance_SideEffectsConclusion", "Most common criteria of good running form do not meet these requirements.")),
            References = ReferencesByNumber(references, "3", "6", "7"),
            SortOrder = 31
        };

        yield return new AcademyArticle
        {
            Id = "running-analysis-shared-decision",
            Title = RunningAnalysisText("InterventionGuidance_ConsentPowerDynamicsTitle", "Shared decision-making"),
            Summary = RunningAnalysisText("InterventionGuidance_Talk_4", "You receive an interpretation and options, not an obligation to correct."),
            Category = AcademyArticleCategories.RunningAnalysis,
            SourceModule = "Running analysis guidance",
            Tags = commonTags,
            BodyBlocks = Blocks(
                RunningAnalysisText("InterventionGuidance_ConsentPowerDynamicsText", "Running-style interventions require informed consent and context."),
                RunningAnalysisText("InterventionGuidance_Talk_1", "A note about running style is not a judgment."),
                RunningAnalysisText("InterventionGuidance_Talk_3", "Running style belongs in the context of symptoms, training, shoes, pace, volume, and fatigue.")),
            References = references,
            SortOrder = 32
        };
    }

    private IEnumerable<AcademyArticle> BuildTrainingPlanArticles()
    {
        foreach (var plan in _trainingPlanService.LoadTrainingPlans())
        {
            var effects = plan.Sessions
                .Select(session => session.TrainingEffect)
                .Where(effect => effect is not null && !effect.IsEmpty)
                .Select(effect => effect.Normalize())
                .Distinct()
                .Take(3)
                .ToList();
            var warmups = plan.Sessions
                .SelectMany(session => session.Warmup)
                .Select(activity => activity.Title)
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(6)
                .ToList();
            var drills = plan.Sessions
                .SelectMany(session => session.Drills)
                .Select(activity => activity.Title)
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(6)
                .ToList();

            yield return new AcademyArticle
            {
                Id = $"training-plan-{Slug(plan.Id)}",
                Title = plan.Name,
                Summary = BuildPlanSummary(plan),
                Category = AcademyArticleCategories.TrainingPlans,
                SourceModule = "Training plan catalog",
                Tags = NormalizeTags(new[] { "Training plan", ExtractLevelTag(plan.Name) }),
                BodyBlocks = Blocks(
                    BuildPlanSummary(plan),
                    BuildTrainingEffectBlock(effects),
                    warmups.Count == 0 ? string.Empty : $"Warm-up: {string.Join(", ", warmups)}",
                    drills.Count == 0 ? string.Empty : $"Drills: {string.Join(", ", drills)}"),
                References = Array.Empty<ContentReference>(),
                SortOrder = 90
            };
        }
    }

    private static string BuildPlanSummary(TrainingPlan plan)
    {
        var dateRange = plan.StartDate is null || plan.EndDate is null
            ? string.Empty
            : $"{plan.StartDate:dd.MM.yyyy} - {plan.EndDate:dd.MM.yyyy}";
        var distance = plan.TotalRunDistance > 0
            ? $", {plan.TotalRunDistance / 1000m:0.#} km running volume"
            : string.Empty;

        return $"{plan.Sessions.Count} sessions{distance}{(string.IsNullOrWhiteSpace(dateRange) ? string.Empty : $" ({dateRange})")}.";
    }

    private static string BuildTrainingEffectBlock(IReadOnlyList<TrainingEffect> effects)
    {
        if (effects.Count == 0)
            return string.Empty;

        return string.Join(
            " ",
            effects.Select(effect =>
                string.Join(
                    " ",
                    Blocks(
                        effect.Focus,
                        effect.Stimulus,
                        effect.Adaptation,
                        effect.Recovery))));
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

    private static IReadOnlyList<string> NormalizeTags(IEnumerable<string?> tags)
    {
        return tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string ExtractLevelTag(string name)
    {
        var match = LevelRegex().Match(name);
        return match.Success ? match.Value : string.Empty;
    }

    private static string Slug(string value)
    {
        var slug = new string(value
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray());

        return string.Join("-", slug.Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    [GeneratedRegex("Level\\s+[0-5]", RegexOptions.IgnoreCase)]
    private static partial Regex LevelRegex();
}
