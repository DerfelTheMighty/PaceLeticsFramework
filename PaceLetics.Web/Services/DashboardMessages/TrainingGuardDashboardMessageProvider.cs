using MudBlazor;
using PaceLetics.CoreModule.Infrastructure.Models;
using PaceLetics.CoreModule.Infrastructure.Services;

namespace PaceLetics.Web.Services.DashboardMessages;

public sealed class TrainingGuardDashboardMessageProvider : IAthleteMessageProvider
{
    private readonly DashboardMessageFeedOptions _options;
    private readonly ITrainingPlanService _trainingPlanService;
    private readonly TrainingGuardEvaluator _evaluator;

    public TrainingGuardDashboardMessageProvider(
        DashboardMessageFeedOptions options,
        ITrainingPlanService trainingPlanService,
        TrainingGuardEvaluator evaluator)
    {
        _options = options;
        _trainingPlanService = trainingPlanService;
        _evaluator = evaluator;
    }

    public void Enqueue(AthleteMessageContext context, AthleteMessageQueue queue)
    {
        var plans = _trainingPlanService.LoadTrainingPlans();
        foreach (var finding in _evaluator.Evaluate(context, plans, _options))
        {
            queue.Enqueue(ToMessage(finding));
        }
    }

    private static AthleteMessage ToMessage(TrainingGuardFinding finding)
    {
        return finding.Type switch
        {
            TrainingGuardFindingType.StaleReferenceBeforeKeySession => new AthleteMessage(
                $"training-guard-reference-stale-{finding.Session.Id}",
                "TrainingGuard",
                Severity.Warning,
                "TrainingGuardReferenceStale_Title",
                "TrainingGuardReferenceStale_Body",
                Icons.Material.Filled.Warning,
                "/Athletes/racepaces",
                "ReferenceRun_Action",
                75,
                new object[]
                {
                    finding.Session.Name,
                    finding.Session.Date,
                    finding.ReferenceAgeDays ?? 0
                }),

            TrainingGuardFindingType.MissingWarmupBeforeKeySession => new AthleteMessage(
                $"training-guard-warmup-missing-{finding.Session.Id}",
                "TrainingGuard",
                Severity.Info,
                "TrainingGuardWarmupMissing_Title",
                "TrainingGuardWarmupMissing_Body",
                Icons.Material.Filled.FitnessCenter,
                "/Athletes/courses",
                "UpcomingTraining_Action",
                55,
                new object[] { finding.Session.Name, finding.Session.Date }),

            _ => throw new InvalidOperationException($"Unsupported training guard finding '{finding.Type}'.")
        };
    }
}
