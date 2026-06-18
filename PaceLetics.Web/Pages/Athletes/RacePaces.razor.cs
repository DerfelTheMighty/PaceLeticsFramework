
using MudBlazor;
using MudBlazor.Extensions;
using Microsoft.AspNetCore.Components;
using PaceLetics.AthleteModule.CodeBase.Models;
using PaceLetics.AthleteModule.Components;
using PaceLetics.CoreModule.Infrastructure.Interfaces;
using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.Web.Pages.Athletes
{
    public partial class RacePaces
    {
        private AthleteModel _athlete = new AthleteModel();
        private PaceModel _upperPace = new();
        private PaceModel _lowerPace = new();
        private PaceModel _upperCriticalSpeedPace = new();
        private PaceModel _lowerCriticalSpeedPace = new();
        private CriticalSpeedModel _criticalSpeed = new();
        private bool _hasDanielsPaceModel;
        private bool _hasCriticalSpeedPaceModel;
        private double[] _vdotData { get; set; } = {0, 0};

        [Parameter]
        [SupplyParameterFromQuery(Name = "section")]
        public string? Section { get; set; }

        protected override void OnParametersSet()
        {
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

        private async Task AddAthlete()
        {
            await OpenRaceDialog(_athlete.ActiveReferenceResult);
        }

        private async Task AddReferenceRun()
        {
            await OpenRaceDialog(new RaceResultModel { Date = DateTime.Now });
        }

        private async Task OpenRaceDialog(RaceResultModel? raceResult)
        {
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small };

            var parameters = new DialogParameters<AddRaceDialog> { { x => x.Model, raceResult ?? new RaceResultModel() } };

            var dialogRef = await dialogService.ShowAsync<AddRaceDialog>(string.Empty, parameters, options);
            var result = await dialogRef.Result;

            if (result is null || result.Canceled)
                return;

            var rrm = result.Data as RaceResultModel;
            if (rrm is null)
                return;

            _athlete.ActiveReferenceResult = rrm;
            SaveRaceResult(rrm);
            UpdateDanielsModel(rrm);
            UpdatePaceModels();

            await Loading.RunAsync(L["Loading"], () => AthleteData.UpdateAthlete(_athlete));
        }

        private void ApplyAthleteData(AthleteModel athlete)
        {
            _athlete = athlete;
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
                _upperCriticalSpeedPace = new PaceModel();
                _lowerCriticalSpeedPace = new PaceModel();
                _hasCriticalSpeedPaceModel = false;
                return;
            }

            _upperCriticalSpeedPace = criticalSpeedService.BuildPaceModel(_criticalSpeed);
            _lowerCriticalSpeedPace = _upperCriticalSpeedPace.Reduce(0.975);
            _hasCriticalSpeedPaceModel = true;
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

        private void SaveRaceResult(RaceResultModel result)
        {
            _athlete.RaceResults ??= new List<RaceResultModel>();

            var existingIndex = _athlete.RaceResults.FindIndex(existing =>
                !string.IsNullOrWhiteSpace(result.Id)
                && string.Equals(existing.Id, result.Id, StringComparison.OrdinalIgnoreCase));

            if (existingIndex >= 0)
            {
                _athlete.RaceResults[existingIndex] = result;
                return;
            }

            _athlete.RaceResults.Add(result);
        }

        private IEnumerable<RaceResultModel> GetCriticalSpeedResults()
        {
            var results = new List<RaceResultModel>();

            if (_athlete.RaceResults is not null)
                results.AddRange(_athlete.RaceResults);

            if (_athlete.ActiveReferenceResult is not null && !results.Any(result => IsSameRaceResult(result, _athlete.ActiveReferenceResult)))
                results.Add(_athlete.ActiveReferenceResult);

            return results;
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
                "info" or "infos" => RacePacesSection.Info,
                _ => RacePacesSection.RaceData
            };

            return normalized is "pacedata" or "racedata" or "laufdaten" or "pacezones" or "paces" or "pacebereiche" or "info" or "infos";
        }
    }
}
