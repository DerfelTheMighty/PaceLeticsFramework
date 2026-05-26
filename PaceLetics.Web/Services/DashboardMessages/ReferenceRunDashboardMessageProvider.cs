using MudBlazor;
using PaceLetics.CoreModule.Infrastructure.Models;
using PaceLetics.CoreModule.Infrastructure.Services;

namespace PaceLetics.Web.Services.DashboardMessages;

public sealed class ReferenceRunDashboardMessageProvider : IAthleteMessageProvider
{
    private readonly DashboardMessageFeedOptions _options;

    public ReferenceRunDashboardMessageProvider(DashboardMessageFeedOptions options)
    {
        _options = options;
    }

    public void Enqueue(AthleteMessageContext context, AthleteMessageQueue queue)
    {
        var referenceResult = context.ActiveReferenceResult;

        if (referenceResult is null || !HasUsableReferenceResult(referenceResult))
        {
            queue.Enqueue(new AthleteMessage(
                "reference-run-missing",
                "Athlete",
                Severity.Warning,
                "ReferenceRunMissing_Title",
                "ReferenceRunMissing_Body",
                Icons.Material.Filled.DirectionsRun,
                "/Athletes/racepaces",
                "ReferenceRun_Action",
                100,
                Array.Empty<object>()));
            return;
        }

        var age = context.Today.Date - referenceResult.Date.Date;
        if (age > _options.ReferenceRunMaxAge)
        {
            queue.Enqueue(new AthleteMessage(
                "reference-run-stale",
                "Athlete",
                Severity.Info,
                "ReferenceRunStale_Title",
                "ReferenceRunStale_Body",
                Icons.Material.Filled.Update,
                "/Athletes/racepaces",
                "ReferenceRun_Action",
                80,
                new object[] { referenceResult.Date, Math.Floor(age.TotalDays) }));
        }
    }

    private static bool HasUsableReferenceResult(RaceResultModel? referenceResult)
    {
        return referenceResult is not null
            && referenceResult.DistanceM > 0
            && referenceResult.Time > TimeSpan.Zero;
    }
}
