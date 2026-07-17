using PaceLetics.TrainingPlanModule.CodeBase.Models;
using PaceLetics.Web.Services.DashboardPreferences;

namespace PaceLetics.Tests;

public sealed class PriorityTwoUxTests
{
    [Fact]
    public void DashboardPreferences_NormalizePreservesOrderAndAddsNewSections()
    {
        var normalized = DashboardPreferencesService.Normalize(
        [
            new DashboardSectionPreference(DashboardSectionKeys.Progress, true, 4),
            new DashboardSectionPreference(DashboardSectionKeys.NextSession, false, 1),
            new DashboardSectionPreference("unknown", true, 0)
        ]);

        Assert.Equal(DashboardSectionKeys.NextSession, normalized[0].Key);
        Assert.False(normalized[0].Visible);
        Assert.Equal(DashboardSectionKeys.Progress, normalized[1].Key);
        Assert.Equal(DashboardSectionKeys.Defaults.Count, normalized.Count);
        Assert.Equal(Enumerable.Range(0, normalized.Count), normalized.Select(item => item.Order));
        Assert.False(normalized.Single(item => item.Key == DashboardSectionKeys.Messages).Visible);
    }

    [Fact]
    public void DashboardPreferences_CreateDefaultShowsOnlyCoreSections()
    {
        var defaults = DashboardPreferencesService.CreateDefault();

        Assert.Equal(4, defaults.Count(item => item.Visible));
        Assert.False(defaults.Single(item => item.Key == DashboardSectionKeys.Messages).Visible);
        Assert.Equal(
            [
                DashboardSectionKeys.Readiness,
                DashboardSectionKeys.NextSession,
                DashboardSectionKeys.Week,
                DashboardSectionKeys.Progress
            ],
            defaults.Where(item => item.Visible).Select(item => item.Key));
    }

    [Fact]
    public void TrainingSessionAppointment_NormalizeKeepsTrimmedChangeReason()
    {
        var appointment = new TrainingSessionAppointment(ChangeReason: "  Moved due to heat  ").Normalize();

        Assert.Equal("Moved due to heat", appointment.ChangeReason);
        Assert.False(appointment.IsEmpty);
    }
}
