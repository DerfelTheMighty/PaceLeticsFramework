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
    }

    [Fact]
    public void TrainingSessionAppointment_NormalizeKeepsTrimmedChangeReason()
    {
        var appointment = new TrainingSessionAppointment(ChangeReason: "  Moved due to heat  ").Normalize();

        Assert.Equal("Moved due to heat", appointment.ChangeReason);
        Assert.False(appointment.IsEmpty);
    }
}
