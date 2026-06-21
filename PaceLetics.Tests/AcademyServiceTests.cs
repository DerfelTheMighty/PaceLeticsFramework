using System.Globalization;
using Microsoft.Extensions.Localization;
using PaceLetics.RunningAnalysisModule.Components;
using PaceLetics.Web.Pages.Athletes;
using PaceLetics.Web.Services.Academy;
using AcademyPage = PaceLetics.Web.Pages.Academy.Academy;

namespace PaceLetics.Tests;

public sealed class AcademyServiceTests
{
    [Fact]
    public void GetArticles_ReturnsTheThreeAcademyArticles()
    {
        var service = CreateService();

        var articles = service.GetArticles();

        Assert.Collection(
            articles,
            article => Assert.Equal("mental-resource-running", article.Id),
            article => Assert.Equal("evidence-based-running-analysis", article.Id),
            article => Assert.Equal("pace-controlled-training", article.Id));

        var mentalResource = Assert.Single(articles, article => article.Id == "mental-resource-running");
        Assert.Equal(AcademyArticleCategories.Fundamentals, mentalResource.Category);
        Assert.Equal("Laufen als mentale Resource", mentalResource.Title);
        Assert.Equal("Academy mental resource", mentalResource.SourceModule);
        Assert.Contains(mentalResource.BodyBlocks, block => block.Contains("Novice runners"));
        Assert.Contains(mentalResource.References, reference => reference.Url.Contains("ijerph17218059"));
        Assert.Contains(mentalResource.References, reference => reference.Url.Contains("jsams.2018.06.003"));

        var runningAnalysis = Assert.Single(articles, article => article.Id == "evidence-based-running-analysis");
        Assert.Equal(AcademyArticleCategories.RunningAnalysis, runningAnalysis.Category);
        Assert.Equal("Evidenzbasierte Laufanalyse", runningAnalysis.Title);
        Assert.Contains(runningAnalysis.BodyBlocks, block => block.Contains("Technique changes shift load."));
        Assert.Contains(runningAnalysis.References, reference => reference.Url.Contains("31028658"));

        var paceTraining = Assert.Single(articles, article => article.Id == "pace-controlled-training");
        Assert.Equal(AcademyArticleCategories.Training, paceTraining.Category);
        Assert.Equal("Pacegesteuertes Training", paceTraining.Title);
        Assert.Contains(paceTraining.BodyBlocks, block => block.Contains("Pacebereiche"));
    }

    [Fact]
    public void GetCategories_ReturnsCategoriesWithSeedArticles()
    {
        var service = CreateService();

        var categories = service.GetCategories();

        Assert.Contains(AcademyArticleCategories.Fundamentals, categories);
        Assert.Contains(AcademyArticleCategories.RunningAnalysis, categories);
        Assert.Contains(AcademyArticleCategories.Training, categories);
        Assert.DoesNotContain("workouts", categories);
        Assert.DoesNotContain("trainingPlans", categories);
    }

    private static AcademyService CreateService()
    {
        return new AcademyService(
            new DictionaryLocalizer<AcademyPage>(AcademyTexts),
            new DictionaryLocalizer<Dashboard>(DashboardTexts),
            new DictionaryLocalizer<RunningAnalysisResources>(RunningAnalysisTexts));
    }

    private static readonly Dictionary<string, string> AcademyTexts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ArticleMentalResourceTitle"] = "Laufen als mentale Resource",
        ["ArticleRunningAnalysisTitle"] = "Evidenzbasierte Laufanalyse",
        ["ArticleRunningAnalysisSummary"] = "Warum Laufanalyse Evidenz, Kontext und gemeinsame Entscheidungen braucht.",
        ["ArticlePaceTrainingTitle"] = "Pacegesteuertes Training",
        ["ArticlePaceTrainingSummary"] = "Warum Pacebereiche Trainingsbelastung steuerbar und nachvollziehbar machen.",
        ["ArticlePaceTrainingBody1"] = "Pacegesteuertes Training übersetzt Leistungsdaten in konkrete Geschwindigkeitsbereiche. Dadurch wird aus einem Plan nicht nur eine Strecke, sondern eine steuerbare Belastung.",
        ["ArticlePaceTrainingBody2"] = "Pacebereiche helfen, lockere Läufe wirklich locker zu halten und harte Einheiten gezielt zu dosieren. Das schützt vor dem typischen Muster, jede Einheit unbewusst mittelhart zu laufen.",
        ["ArticlePaceTrainingBody3"] = "PaceLetics nutzt Referenzleistungen, VDOT und Critical Speed, um Training verständlich zu machen. Die Pace ist dabei kein Selbstzweck, sondern ein Werkzeug zur Orientierung.",
        ["ArticlePaceTrainingBody4"] = "Wenn Form, Müdigkeit, Wetter oder Gelände nicht passen, bleibt Wahrnehmung wichtig. Gute Trainingssteuerung verbindet Zahlen mit Körpergefühl."
    };

    private static readonly Dictionary<string, string> DashboardTexts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["MentalResource_Title"] = "Why running is more than training",
        ["MentalResource_Lead"] = "Running can become a controllable resource.",
        ["MentalResource_StepEffectText"] = "Running can support mood.",
        ["MentalResource_StepAppText"] = "PaceLetics makes routines understandable.",
        ["MentalResource_StepFragileTitle"] = "Not available without limits",
        ["MentalResource_StepFragileText"] = "Running depends on load tolerance.",
        ["MentalResource_StepConsequenceText"] = "Sustainable programs cannot treat drop-out as a side issue.",
        ["MentalResource_StepRiskTitle"] = "Where risk is higher for beginners",
        ["MentalResource_StepRiskText"] = "Novice runners have higher injury incidence.",
        ["MentalResource_SourceOswald"] = "Oswald et al.",
        ["MentalResource_SourcePereira"] = "Pereira et al.",
        ["MentalResource_SourceFurie"] = "Furie et al.",
        ["MentalResource_SourceDeJonge"] = "de Jonge et al.",
        ["MentalResource_SourceVerhagen"] = "Verhagen et al.",
        ["MentalResource_SourceVidebaek"] = "Videbaek et al.",
        ["MentalResource_SourceFokkema"] = "Fokkema et al.",
        ["MentalResource_SourceRelph"] = "Relph et al."
    };

    private static readonly Dictionary<string, string> RunningAnalysisTexts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["InterventionGuidance_Title"] = "Intervention philosophy",
        ["InterventionGuidance_Subtitle"] = "No intervention without indication.",
        ["InterventionGuidance_CompactIntro"] = "Running style is a self-optimizing system.",
        ["InterventionGuidance_SystemUnderstandingText"] = "Bodies differ.",
        ["InterventionGuidance_FunctioningSystemAlert"] = "Symptom-free style can be functional.",
        ["InterventionGuidance_SideEffectsTitle"] = "Interventions have side effects",
        ["InterventionGuidance_SideEffectsIntro"] = "Technique changes shift load.",
        ["InterventionGuidance_SideEffectsIndicationIntro"] = "Interventions need an indication.",
        ["InterventionGuidance_SideEffectsIndication_1"] = "Moderate to good evidence.",
        ["InterventionGuidance_SideEffectsIndication_2"] = "Symptoms and plausible links.",
        ["InterventionGuidance_SideEffectsConclusion"] = "Most criteria do not meet this.",
        ["InterventionGuidance_ConsentPowerDynamicsTitle"] = "Shared decision-making",
        ["InterventionGuidance_Talk_4"] = "Options, not correction duty.",
        ["InterventionGuidance_ConsentPowerDynamicsText"] = "Use informed consent.",
        ["InterventionGuidance_Talk_1"] = "No right or wrong judgment.",
        ["InterventionGuidance_Talk_3"] = "Context matters.",
        ["InterventionGuidance_SourceCeyssens"] = "Ceyssens et al.",
        ["InterventionGuidance_SourceWillwacher"] = "Willwacher et al.",
        ["InterventionGuidance_SourceAndersonStepRate"] = "Anderson et al.",
        ["InterventionGuidance_SourceMoore"] = "Moore.",
        ["InterventionGuidance_SourceNigg"] = "Nigg et al.",
        ["InterventionGuidance_SourceGaitRetraining"] = "Gait retraining.",
        ["InterventionGuidance_SourceRidge"] = "Ridge et al.",
        ["InterventionGuidance_SourceChristopher"] = "Christopher et al."
    };

    private sealed class DictionaryLocalizer<T> : IStringLocalizer<T>
    {
        private readonly IReadOnlyDictionary<string, string> _values;

        public DictionaryLocalizer(IReadOnlyDictionary<string, string> values)
        {
            _values = values;
        }

        public LocalizedString this[string name]
        {
            get
            {
                var found = _values.TryGetValue(name, out var value);
                return new LocalizedString(name, found ? value! : name, resourceNotFound: !found);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var localized = this[name];
                return localized.ResourceNotFound
                    ? localized
                    : new LocalizedString(name, string.Format(CultureInfo.CurrentCulture, localized.Value, arguments));
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return _values.Select(pair => new LocalizedString(pair.Key, pair.Value));
        }
    }
}
