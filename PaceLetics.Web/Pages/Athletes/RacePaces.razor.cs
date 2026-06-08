
using MudBlazor;
using MudBlazor.Extensions;
using PaceLetics.AthleteModule.CodeBase.Models;
using PaceLetics.AthleteModule.Components;
using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.Web.Pages.Athletes
{
    public partial class RacePaces
    {
        private AthleteModel _athlete = new AthleteModel();
        private PaceModel _upperPace = new();
        private PaceModel _lowerPace = new();
        private bool _hasPaceModel;
        private double[] _vdotData { get; set; } = {0, 0};

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
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small };

            RaceResultModel? res = _athlete.ActiveReferenceResult;
            var parameters = new DialogParameters<AddRaceDialog> { { x => x.Model, res } };

            var dialogRef = await dialogService.ShowAsync<AddRaceDialog>(string.Empty, parameters, options);
            var result = await dialogRef.Result;

            if (result is null || result.Canceled)
                return;

            var rrm = result.Data as RaceResultModel;
            if (rrm is null)
                return;

            _athlete.ActiveReferenceResult = rrm;
            _athlete.Vdot = vdotService.GetVdot(rrm);
            _vdotData[0] = _athlete.Vdot;
            _vdotData[1] = 85 - _athlete.Vdot; // TODO: Magic number
            _athlete.PaceModel = pmProvider[_athlete.Vdot];
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
            if (_athlete.Vdot < 1 || _athlete.PaceModel is null)
            {
                _upperPace = new PaceModel();
                _lowerPace = new PaceModel();
                _hasPaceModel = false;
                return;
            }

            _upperPace = _athlete.PaceModel;
            _lowerPace = _athlete.PaceModel.Reduce(0.975);
            _hasPaceModel = true;
        }
    }
}
