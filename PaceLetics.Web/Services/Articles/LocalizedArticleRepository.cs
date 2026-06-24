using Microsoft.Extensions.Localization;
using PaceLetics.AthleteModule.Components;
using PaceLetics.RunningAnalysisModule.Components;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;
using PaceLetics.Web.Pages.Athletes;
using AcademyPage = PaceLetics.Web.Pages.Academy.Academy;

namespace PaceLetics.Web.Services.Articles;

public sealed class LocalizedArticleRepository : IArticleRepository
{
    private readonly IStringLocalizer<AcademyPage> _academyLocalizer;
    private readonly IStringLocalizer<AthleteResources> _athleteLocalizer;
    private readonly IStringLocalizer<Dashboard> _dashboardLocalizer;
    private readonly IStringLocalizer<RunningAnalysisResources> _runningAnalysisLocalizer;

    public LocalizedArticleRepository(
        IStringLocalizer<AcademyPage> academyLocalizer,
        IStringLocalizer<AthleteResources> athleteLocalizer,
        IStringLocalizer<Dashboard> dashboardLocalizer,
        IStringLocalizer<RunningAnalysisResources> runningAnalysisLocalizer)
    {
        _academyLocalizer = academyLocalizer;
        _athleteLocalizer = athleteLocalizer;
        _dashboardLocalizer = dashboardLocalizer;
        _runningAnalysisLocalizer = runningAnalysisLocalizer;
    }

    public IReadOnlyList<Article> GetArticles()
    {
        return BuildArticles().ToList();
    }

    private IEnumerable<Article> BuildArticles()
    {
        foreach (var article in BuildMentalResourceArticles())
            yield return article;

        foreach (var article in BuildRunningAnalysisArticles())
            yield return article;

        foreach (var article in BuildPaceControlledTrainingArticles())
            yield return article;
    }

    private IEnumerable<Article> BuildMentalResourceArticles()
    {
        var references = BuildMentalResourceReferences();
        var lead = DashboardText("MentalResource_Lead", "Running can become a psychological resource when load, recovery, and routine fit together.");

        yield return new Article
        {
            Id = "mental-resource-running",
            Title = AcademyText("ArticleMentalResourceTitle", "Laufen als mentale Resource"),
            Summary = lead,
            Category = ArticleCategories.Fundamentals,
            SourceModule = "MentalResourceArticle",
            Tags = new[] { "Mental resource", "Literature", "Mood", "Self-efficacy" },
            BodyBlocks = Blocks(
                lead,
                DashboardText("MentalResource_ResearchIntro", "The current literature suggests that running can be a low-threshold mental resource."),
                DashboardText("MentalResource_ResearchOswald", "A scoping review on running and mental health links single runs and regular running with better mood, lower tension, and positive mental health outcomes."),
                DashboardText("MentalResource_ResearchPereira", "A systematic review on recreational running highlights motivation, self-regulation, habit, and perceived competence as central psychological factors for staying with running."),
                DashboardText("MentalResource_ResearchActivityReviews", "Broader reviews support this interpretation: physical activity reduces depressive symptoms, anxiety, and psychological distress, and walking or jogging is among the well-studied forms of movement for depressive symptoms."),
                DashboardText("MentalResource_ResearchCochrane", "The current Cochrane review concludes that exercise can moderately reduce depressive symptoms compared with control conditions, while long-term evidence remains limited."),
                DashboardText("MentalResource_ResearchProspective", "Prospective cohort data also show that even moderate activity volumes are associated with a lower risk of depression."),
                DashboardText("MentalResource_ResearchConclusion", "Running should not be inflated into therapy or a therapy substitute. The point is that running can be a mental resource and that many people intuitively use it as one.")),
            References = references,
            ContentKind = ArticleContentKind.MentalResource,
            SortOrder = 10
        };
    }

    private IEnumerable<Article> BuildRunningAnalysisArticles()
    {
        var references = BuildRunningAnalysisReferences();

        yield return new Article
        {
            Id = "evidence-based-running-analysis",
            Title = AcademyText("ArticleRunningAnalysisTitle", "Evidenzbasierte Laufanalyse"),
            Summary = AcademyText("ArticleRunningAnalysisSummary", "Warum Laufanalyse Evidenz, Kontext und gemeinsame Entscheidungen braucht."),
            Category = ArticleCategories.RunningAnalysis,
            SourceModule = "RunningAnalysisInterventionGuidance",
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
            ContentKind = ArticleContentKind.RunningAnalysisGuidance,
            SortOrder = 20
        };
    }

    private IEnumerable<Article> BuildPaceControlledTrainingArticles()
    {
        var references = BuildPaceModelReferences();

        yield return new Article
        {
            Id = "pace-controlled-training",
            Title = AcademyText("ArticlePaceTrainingTitle", "Pacegesteuertes Training"),
            Summary = DashboardText("Metric_TrainingSystemDetail", "Why we train pace-guided"),
            Category = ArticleCategories.Training,
            SourceModule = "PaceModelInfo",
            Tags = new[] { "Pace", "VDOT", "Critical speed", "D'", "Training load" },
            BodyBlocks = Blocks(
                AthleteText("PaceModelInfo_Rationale_PaceProxy_Text", "Pace is a practical proxy for external training load."),
                AthleteText("PaceModelInfo_Rationale_Adaptation_Text", "Training adaptations happen on different timelines."),
                AthleteText("PaceModelInfo_Rationale_Remaining_Text", "Good training control keeps useful stress and available adaptation in balance."),
                AthleteText("PaceModelInfo_Daniels_Text", "Daniels/VDOT translates current performance into standardized training paces."),
                AthleteText("PaceModelInfo_Cs_Text", "Critical Speed describes the boundary between heavy and severe intensity domains."),
                AthleteText("PaceModelInfo_Calculation_Text", "PaceLetics calculates with speed internally and uses VDOT, Critical Speed, and D' for training guidance.")),
            References = references,
            ContentKind = ArticleContentKind.PaceModelInfo,
            SortOrder = 30
        };
    }

    private List<ContentReference> BuildMentalResourceReferences()
    {
        return new List<ContentReference>
        {
            Reference("MentalResource_SourceOswald", "https://doi.org/10.3390/ijerph17218059", _dashboardLocalizer),
            Reference("MentalResource_SourcePereira", "https://doi.org/10.3389/fpsyg.2021.624783", _dashboardLocalizer),
            Reference("MentalResource_SourceSingh", "https://doi.org/10.1136/bjsports-2022-106195", _dashboardLocalizer),
            Reference("MentalResource_SourceNoetel", "https://doi.org/10.1136/bmj-2023-075847", _dashboardLocalizer),
            Reference("MentalResource_SourceCochrane", "https://doi.org/10.1002/14651858.CD004366.pub7", _dashboardLocalizer),
            Reference("MentalResource_SourcePearce", "https://doi.org/10.1001/jamapsychiatry.2022.0609", _dashboardLocalizer)
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

    private List<ContentReference> BuildPaceModelReferences()
    {
        return new List<ContentReference>
        {
            Reference("PaceModelInfo_Anderson_Title", "https://link.springer.com/article/10.1007/s40279-026-02410-x", _athleteLocalizer, "1"),
            Reference("PaceModelInfo_Lipkova_Title", "https://pmc.ncbi.nlm.nih.gov/articles/PMC11933073/", _athleteLocalizer, "2"),
            Reference("PaceModelInfo_Hawley_Title", "https://pubmed.ncbi.nlm.nih.gov/28490537/", _athleteLocalizer, "3"),
            Reference("PaceModelInfo_MacInnis_Title", "https://pubmed.ncbi.nlm.nih.gov/27748956/", _athleteLocalizer, "4"),
            Reference("PaceModelInfo_Kubo_Title", "https://paulogentil.com/pdf/Time%20Course%20of%20Changes%20in%20Muscle%20and%20Tendon%20Properties%20During%20Strength%20Training%20and%20Detraining.pdf", _athleteLocalizer, "5"),
            Reference("PaceModelInfo_Bohm_Title", "https://www.germanjournalsportsmedicine.com/archive/archive-2019/issue-4/functional-adaptation-of-connective-tissue-by-training/", _athleteLocalizer, "6"),
            Reference("PaceModelInfo_Bohm2015_Title", "https://link.springer.com/article/10.1186/s40798-015-0009-9", _athleteLocalizer, "7"),
            Reference("PaceModelInfo_Papagiannaki_Title", "https://www.frontiersin.org/journals/bioengineering-and-biotechnology/articles/10.3389/fbioe.2020.533391/full", _athleteLocalizer, "8"),
            Reference("PaceModelInfo_Jiang_Title", "https://www.frontiersin.org/journals/bioengineering-and-biotechnology/articles/10.3389/fbioe.2024.1378284/full", _athleteLocalizer, "9"),
            Reference("PaceModelInfo_Billat_Title", "https://www.frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2019.03026/full", _athleteLocalizer, "10"),
            Reference("PaceModelInfo_Warden_Title", "https://pubmed.ncbi.nlm.nih.gov/33635519/", _athleteLocalizer, "11")
        };
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

    private string AcademyText(string key, string fallback)
    {
        return Localized(_academyLocalizer, key, fallback);
    }

    private string AthleteText(string key, string fallback)
    {
        return Localized(_athleteLocalizer, key, fallback);
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
}
