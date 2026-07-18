using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.CoreModule.Infrastructure.Interfaces
{
    public interface ICriticalSpeedService
    {
        CriticalSpeedModel Estimate(IEnumerable<RaceResultModel> results);

        IReadOnlyList<RaceResultModel> GetContributingResults(IEnumerable<RaceResultModel> results);

        EnduranceProjectionModel BuildEnduranceProjection(
            CriticalSpeedModel model,
            IEnumerable<RaceResultModel> results,
            double anchorDistanceMeters);

        PaceModel BuildPaceModel(CriticalSpeedModel model);

        IReadOnlyList<CriticalSpeedIntervalRecommendation> BuildIntervalRecommendations(CriticalSpeedModel model);
    }
}
