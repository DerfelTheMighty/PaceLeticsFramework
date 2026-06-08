using MudBlazor;
using PaceLetics.CoreModule.Infrastructure.Models;
using PaceLetics.CoreModule.Infrastructure.Services;
using PaceLetics.TrainingPlanModule.CodeBase.Models;
using PaceLetics.Web.Services;

namespace PaceLetics.Web.Services.DashboardMessages;

public sealed class UpcomingTrainingDashboardMessageProvider : IAthleteMessageProvider
{
    private readonly DashboardMessageFeedOptions _options;
    private readonly ITrainingPlanService _trainingPlanService;

    public UpcomingTrainingDashboardMessageProvider(
        DashboardMessageFeedOptions options,
        ITrainingPlanService trainingPlanService)
    {
        _options = options;
        _trainingPlanService = trainingPlanService;
    }

    public void Enqueue(AthleteMessageContext context, AthleteMessageQueue queue)
    {
        var plans = _trainingPlanService.LoadTrainingPlans();
        var upcoming = FindUpcomingSession(context, plans);
        if (upcoming is null)
            return;

        var daysUntil = (upcoming.Date.Date - context.Today.Date).Days;
        if (daysUntil > _options.UpcomingTrainingLookAhead.TotalDays)
            return;

        var bodyKey = daysUntil switch
        {
            0 => "UpcomingTrainingToday_Body",
            1 => "UpcomingTrainingTomorrow_Body",
            _ => "UpcomingTraining_Body"
        };

        queue.Enqueue(new AthleteMessage(
            $"upcoming-training-{upcoming.Id}",
            "TrainingPlan",
            Severity.Info,
            "UpcomingTraining_Title",
            bodyKey,
            Icons.Material.Filled.EventAvailable,
            "/Athletes/courses",
            "UpcomingTraining_Action",
            40,
            new object[] { upcoming.Name, upcoming.Date, daysUntil }));
    }

    private static TrainingSession? FindUpcomingSession(
        AthleteMessageContext context,
        IReadOnlyList<TrainingPlan> plans)
    {
        if (plans.Count == 0)
            return null;

        var selectedPlanId = context.SelectedTrainingPlanId;
        var selectedPlan = string.IsNullOrWhiteSpace(selectedPlanId)
            ? null
            : plans.FirstOrDefault(plan => plan.Id == selectedPlanId);

        return selectedPlan is not null
            ? FindNextSession(selectedPlan, context.Today)
            : plans
                .Select(plan => FindNextSession(plan, context.Today))
                .Where(session => session is not null)
                .OrderBy(session => session!.Date)
                .FirstOrDefault();
    }

    private static TrainingSession? FindNextSession(TrainingPlan plan, DateTime today)
    {
        return plan.Sessions
            .Where(session => session.Date.Date >= today.Date)
            .OrderBy(session => session.Date)
            .FirstOrDefault();
    }
}
