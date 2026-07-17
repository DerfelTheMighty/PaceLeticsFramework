
using MudBlazor;
using Microsoft.AspNetCore.Components;
using PaceLetics.AthleteModule.CodeBase.Models;
using PaceLetics.AthleteModule.Components;
using PaceLetics.CoreModule.Infrastructure.Interfaces;
using PaceLetics.CoreModule.Infrastructure.Models;
using PaceLetics.Web.Services;

namespace PaceLetics.Web.Pages.Athletes
{
    public partial class RacePaces
    {
        private AthleteModel _athlete = new AthleteModel();
        private PaceModel _upperPace = new();
        private PaceModel _lowerPace = new();
        private CriticalSpeedModel _criticalSpeed = new();
        private IReadOnlyList<CriticalSpeedIntervalRecommendation> _criticalSpeedIntervals = [];
        private bool _hasDanielsPaceModel;
        private double[] _vdotData { get; set; } = {0, 0};
        private IReadOnlyList<RaceResultModel> ReferenceRuns => GetReferenceRuns()
            .OrderByDescending(result => result.Date)
            .ThenBy(result => result.Id, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        [Parameter]
        [SupplyParameterFromQuery(Name = "section")]
        public string? Section { get; set; }

        [Parameter]
        public bool Embedded { get; set; }

        [Parameter]
        public string? EmbeddedSection { get; set; }

        [Inject] private NavigationManager Navigation { get; set; } = default!;

        private string ContainerClass => Embedded
            ? "pa-0"
            : "pl-section-page px-6 pb-8";

        protected override void OnParametersSet()
        {
            if (Embedded && TryParseSection(EmbeddedSection, out var embeddedSection))
            {
                _activeSection = embeddedSection;
                return;
            }

            if (Navigation.ToBaseRelativePath(Navigation.Uri)
                .Split('?', '#')[0]
                .Equals("Athletes/pacezones", StringComparison.OrdinalIgnoreCase))
            {
                _activeSection = RacePacesSection.PaceZones;
                return;
            }

            if (TryParseSection(Section, out var section))
                _activeSection = section;
        }

        protected override async Task OnInitializedAsync()
        {
            await Loading.RunAsync(L["Loading"], async () =>
            {
                try
                {
                    var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
                    var user = authstate.User;
                    var id = user.FindFirst(u => u.Type.Contains("nameidentifier"))?.Value;

                    if (!string.IsNullOrEmpty(id))
                    {
                        var athlete = await AthleteData.GetAthlete(id);
                        if (athlete is null)
                            return;

                        ApplyAthleteData(athlete);
                    }
                }
                catch (Exception ex)
                {
                    await dialogService.ShowInformationAsync(
                        L["LoadError_Title"],
                        $"{L["LoadError_Message"]}\n{ex.Message}",
                        Icons.Material.Filled.Error);
                }
            });
        }

        private async Task EditReferenceRun(RaceResultModel raceResult)
        {
            await OpenRaceDialog(raceResult);
        }

        private async Task AddReferenceRun()
        {
            await OpenRaceDialog(null);
        }

        private async Task DeleteReferenceRun(RaceResultModel raceResult)
        {
            var confirmed = await JS.InvokeAsync<bool>("confirm", new object?[] { L["DeleteRaceConfirm"].Value });
            if (!confirmed)
                return;

            var removedActiveReference = IsActiveReferenceRun(raceResult);
            RemoveRaceResult(raceResult);

            if (removedActiveReference)
            {
                _athlete.ActiveReferenceResult = _athlete.RaceResults
                    .OrderByDescending(result => result.Date)
                    .FirstOrDefault();

                if (_athlete.ActiveReferenceResult is not null)
                    UpdateDanielsModel(_athlete.ActiveReferenceResult);
                else
                    ResetDanielsModel();
            }

            UpdatePaceModels();

            await Loading.RunAsync(L["Loading"], () => AthleteData.UpdateAthlete(_athlete));
        }

        private async Task OpenRaceDialog(RaceResultModel? raceResult)
        {
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small };
            var dialogModel = raceResult is null
                ? new RaceResultModel { Date = DateTime.Now }
                : CloneRaceResult(raceResult);
            var parameters = new DialogParameters<AddRaceDialog> { { x => x.Model, dialogModel } };

            var dialogRef = await dialogService.ShowAsync<AddRaceDialog>(string.Empty, parameters, options);
            var result = await dialogRef.Result;

            if (result is null || result.Canceled)
                return;

            var rrm = result.Data as RaceResultModel;
            if (rrm is null)
                return;

            var updatesActiveReference = raceResult is null || IsActiveReferenceRun(raceResult);

            if (raceResult is null)
                AddRaceResult(rrm);
            else
                ReplaceRaceResult(raceResult, rrm);

            if (updatesActiveReference)
            {
                _athlete.ActiveReferenceResult = rrm;
                UpdateDanielsModel(rrm);
            }

            UpdatePaceModels();

            await Loading.RunAsync(L["Loading"], () => AthleteData.UpdateAthlete(_athlete));
        }

        private void ApplyAthleteData(AthleteModel athlete)
        {
            _athlete = athlete;
            _athlete.RaceResults ??= [];
            _vdotData[0] = _athlete.Vdot;
            _vdotData[1] = 85 - _athlete.Vdot; // TODO: Magic number

            if (_athlete.PaceModel is null && _athlete.Vdot > 0)
            {
                _athlete.PaceModel = pmProvider[_athlete.Vdot];
            }

            UpdatePaceModels();
        }

        private void UpdatePaceModels()
        {
            if (_athlete.Vdot < 1 || _athlete.PaceModel is null || _athlete.PaceModel.Easy == default)
            {
                _upperPace = new PaceModel();
                _lowerPace = new PaceModel();
                _hasDanielsPaceModel = false;
            }
            else
            {
                _upperPace = _athlete.PaceModel;
                _lowerPace = _athlete.PaceModel.Reduce(0.975);
                _hasDanielsPaceModel = true;
            }

            _criticalSpeed = criticalSpeedService.Estimate(GetCriticalSpeedResults());
            if (!_criticalSpeed.IsValid)
            {
                _criticalSpeedIntervals = [];
                return;
            }

            _criticalSpeedIntervals = criticalSpeedService.BuildIntervalRecommendations(_criticalSpeed);
        }

        private void UpdateDanielsModel(RaceResultModel result)
        {
            try
            {
                _athlete.Vdot = vdotService.GetVdot(result);
                _vdotData[0] = _athlete.Vdot;
                _vdotData[1] = 85 - _athlete.Vdot; // TODO: Magic number
                _athlete.PaceModel = pmProvider[_athlete.Vdot];
            }
            catch
            {
                _athlete.Vdot = 0;
                _vdotData[0] = 0;
                _vdotData[1] = 0;
                _athlete.PaceModel = new PaceModel();
            }
        }

        private void ResetDanielsModel()
        {
            _athlete.Vdot = 0;
            _athlete.PaceModel = new PaceModel();
            _vdotData[0] = 0;
            _vdotData[1] = 0;
        }

        private void AddRaceResult(RaceResultModel result)
        {
            _athlete.RaceResults ??= [];
            _athlete.RaceResults.Add(result);
        }

        private void ReplaceRaceResult(RaceResultModel original, RaceResultModel replacement)
        {
            _athlete.RaceResults ??= [];

            var index = _athlete.RaceResults.FindIndex(existing => ReferenceEquals(existing, original));
            if (index < 0)
                index = _athlete.RaceResults.FindIndex(existing => IsSameRaceResult(existing, original));

            if (index >= 0)
                _athlete.RaceResults[index] = replacement;
            else
                _athlete.RaceResults.Add(replacement);
        }

        private void RemoveRaceResult(RaceResultModel result)
        {
            _athlete.RaceResults ??= [];

            var index = _athlete.RaceResults.FindIndex(existing => ReferenceEquals(existing, result));
            if (index < 0)
                index = _athlete.RaceResults.FindIndex(existing => IsSameRaceResult(existing, result));

            if (index >= 0)
                _athlete.RaceResults.RemoveAt(index);
        }

        private IEnumerable<RaceResultModel> GetCriticalSpeedResults()
        {
            return GetReferenceRuns();
        }

        private List<RaceResultModel> GetReferenceRuns()
        {
            var results = new List<RaceResultModel>();

            if (_athlete.RaceResults is not null)
                results.AddRange(_athlete.RaceResults);

            if (_athlete.ActiveReferenceResult is not null
                && !results.Any(result => IsSameRaceResult(result, _athlete.ActiveReferenceResult)))
            {
                results.Add(_athlete.ActiveReferenceResult);
            }

            return results;
        }

        private bool IsActiveReferenceRun(RaceResultModel result)
        {
            return _athlete.ActiveReferenceResult is not null
                && (ReferenceEquals(_athlete.ActiveReferenceResult, result)
                    || IsSameRaceResult(_athlete.ActiveReferenceResult, result));
        }

        private static RaceResultModel CloneRaceResult(RaceResultModel result)
        {
            return new RaceResultModel
            {
                Id = result.Id,
                Type = result.Type,
                DistanceM = result.DistanceM,
                Date = result.Date,
                Time = result.Time
            };
        }

        private static bool IsSameRaceResult(RaceResultModel left, RaceResultModel right)
        {
            return left.DistanceM == right.DistanceM
                && left.Time == right.Time
                && left.Date.Date == right.Date.Date
                && string.Equals(left.Id, right.Id, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParseSection(string? value, out RacePacesSection section)
        {
            section = RacePacesSection.RaceData;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalized = value.Trim().ToLowerInvariant();
            section = normalized switch
            {
                "pacedata" or "racedata" or "laufdaten" => RacePacesSection.RaceData,
                "pacezones" or "paces" or "pacebereiche" => RacePacesSection.PaceZones,
                _ => RacePacesSection.RaceData
            };

            return normalized is "pacedata" or "racedata" or "laufdaten" or "pacezones" or "paces" or "pacebereiche";
        }
    }
}
