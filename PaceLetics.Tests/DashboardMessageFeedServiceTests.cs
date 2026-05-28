using PaceLetics.AthleteModule.CodeBase.Models;
using PaceLetics.CoreModule.Infrastructure.Models;
using PaceLetics.CoreModule.Infrastructure.Services;
using PaceLetics.TrainingModule.CodeBase.Running.Models;
using PaceLetics.TrainingPlanModule.CodeBase.Models;
using PaceLetics.Web.Services;
using PaceLetics.Web.Services.DashboardMessages;
using MudBlazor;

namespace PaceLetics.Tests;

public sealed class DashboardMessageFeedServiceTests
{
    private static readonly DateTime Today = new(2026, 5, 25);

    [Fact]
    public void Build_AddsMessageWhenReferenceRunIsMissing()
    {
        var athlete = new AthleteModel();
        var service = CreateService();

        var messages = service.Build(CreateContext(athlete));

        var message = Assert.Single(messages);
        Assert.Equal("reference-run-missing", message.Id);
        Assert.Equal(Severity.Warning, message.Severity);
    }

    [Fact]
    public void Build_AddsMessageWhenReferenceRunIsStale()
    {
        var athlete = CreateAthleteWithReferenceRun(Today.AddDays(-181));
        var service = CreateService();

        var messages = service.Build(CreateContext(athlete));

        var message = Assert.Single(messages);
        Assert.Equal("reference-run-stale", message.Id);
        Assert.Equal(Severity.Info, message.Severity);
    }

    [Fact]
    public void Build_AddsUpcomingTrainingMessageFromSelectedPlan()
    {
        var athlete = CreateAthleteWithReferenceRun(Today);
        athlete.SelectedTrainingPlanId = "selected";
        var selectedPlan = CreatePlan("selected", new DateTime(2026, 5, 28), "Selected Session");
        var otherPlan = CreatePlan("other", new DateTime(2026, 5, 26), "Other Session");
        var service = CreateService(new[] { otherPlan, selectedPlan });

        var messages = service.Build(CreateContext(athlete));

        var message = Assert.Single(messages);
        Assert.Equal("upcoming-training-selected-session", message.Id);
        Assert.Equal("Selected Session", message.BodyArguments[0]);
    }

    [Fact]
    public void Build_OrdersHigherPriorityMessagesBeforeUpcomingTraining()
    {
        var athlete = new AthleteModel { SelectedTrainingPlanId = "selected" };
        var plan = CreatePlan("selected", new DateTime(2026, 5, 28), "Selected Session");
        var service = CreateService(new[] { plan });

        var messages = service.Build(CreateContext(athlete));

        Assert.Collection(
            messages,
            message => Assert.Equal("reference-run-missing", message.Id),
            message => Assert.Equal("upcoming-training-selected-session", message.Id));
    }

    private static IAthleteMessageFeedService CreateService(IReadOnlyList<TrainingPlan>? plans = null)
    {
        var options = new DashboardMessageFeedOptions();
        return new AthleteMessageFeedService(new IAthleteMessageProvider[]
        {
            new ReferenceRunDashboardMessageProvider(options),
            new UpcomingTrainingDashboardMessageProvider(options, new TestTrainingPlanService(plans ?? Array.Empty<TrainingPlan>()))
        });
    }

    private static AthleteMessageContext CreateContext(AthleteModel athlete)
    {
        return new AthleteMessageContext(
            athlete.AthleteId,
            athlete.Name,
            athlete.Vdot,
            athlete.ActiveReferenceResult,
            athlete.SelectedTrainingPlanId,
            Today);
    }

    private static AthleteModel CreateAthleteWithReferenceRun(DateTime date)
    {
        return new AthleteModel
        {
            ActiveReferenceResult = new RaceResultModel
            {
                Date = date,
                DistanceM = 3000,
                Time = TimeSpan.FromMinutes(15)
            }
        };
    }

    private static TrainingPlan CreatePlan(string id, DateTime date, string sessionName)
    {
        return new TrainingPlan(
            id,
            id,
            new[]
            {
                new TrainingSession(
                    $"{id}-session",
                    sessionName,
                    date,
                    new[] { new TestRunningSession($"{id}-run", sessionName, date) },
                    Array.Empty<WorkoutSessionDefinition>())
            });
    }

    private sealed class TestRunningSession : RunningSession
    {
        public TestRunningSession(string id, string name, DateTime date)
            : base(id, name, date, null, null)
        {
        }

        public override int TotalDistance => 1000;

        public override IReadOnlyList<RunningSegment> Sequence { get; } =
            new[] { new RunningSegment(SegmentType.Dauerlauf, 1000) };
    }

    private sealed class TestTrainingPlanService : ITrainingPlanService
    {
        private readonly IReadOnlyList<TrainingPlan> _plans;

        public TestTrainingPlanService(IReadOnlyList<TrainingPlan> plans)
        {
            _plans = plans;
        }

        public IReadOnlyList<TrainingPlan> LoadTrainingPlans()
        {
            return _plans;
        }

        public Task<IReadOnlyList<TrainingPlan>> LoadTrainingPlansForUserAsync(string? userId)
        {
            return Task.FromResult(_plans);
        }

        public IReadOnlyList<RunningSession> LoadLegacySessions()
        {
            return Array.Empty<RunningSession>();
        }
    }
}
