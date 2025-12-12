
using MudBlazor;
using MudBlazor.Extensions;
using PaceLetics.AthleteModule.CodeBase.Models;
using PaceLetics.VdotModule.CodeBase.Models;
using PaceLetics.VdotModule.Components;

namespace PaceLetics.Web.Pages.Athletes
{
    public partial class RacePaces
    {
        private string _input = string.Empty;
        private bool _isLoading = true;
        private AthleteModel _athlete = new AthleteModel();
        private double[] _vdotData { get; set; } = {0, 0};
        protected override async Task OnInitializedAsync()
        {
            try
            {
                var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
                var user = authstate.User;
                var id = user.FindFirst(u => u.Type.Contains("nameidentifier"))?.Value;

                if (!string.IsNullOrEmpty(id))
                {
                    _athlete = await AthleteData.GetAthlete(id);
                    _vdotData[0] = _athlete.Vdot;
                    _vdotData[1] = 85 - _athlete.Vdot; // TODO: Magic number
                }
            }
            catch (Exception ex)
            {
                await dialogService.ShowInformationAsync(
                    "Fehler beim Laden",
                    $"Deine Daten konnten nicht geladen werden:\n{ex.Message}",
                    Icons.Material.Filled.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task AddAthlete()
        {
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small };

            RaceResultModel? res = _athlete.ActiveReferenceResult;
            var parameters = new DialogParameters<AddRaceDialog> { { x => x.Model, res } };

            var dialogRef = dialogService.Show<AddRaceDialog>(string.Empty, parameters, options);
            var result = await dialogRef.Result;

            if (result.Canceled)
                return;

            var rrm = result.Data as RaceResultModel;
            if (rrm is null)
                return;

            _athlete.ActiveReferenceResult = rrm;
            _athlete.Vdot = vdotService.GetVdot(rrm);
            _vdotData[0] = _athlete.Vdot;
            _vdotData[1] = 85 - _athlete.Vdot; // TODO: Magic number
            _athlete.PaceModel = pmProvider[_athlete.Vdot];

            _isLoading = true;
            try
            {
                await AthleteData.UpdateAthlete(_athlete);
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}