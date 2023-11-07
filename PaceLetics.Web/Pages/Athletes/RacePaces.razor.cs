using CoreLibrary.Models.Athlet;
using CoreLibrary.Models.Race;
using MudBlazor;
using PaceLetics.Components.Race;


namespace PaceLetics.Web.Pages.Athletes
{
    public partial class RacePaces
    {
        private string _input = string.Empty;
        private bool _isLoading = true;
        private AthleteModel _athlete = new AthleteModel();
        private double[] _vdotData { get; set; } = {0, 0};
        protected async override void OnInitialized()
        {
            var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
            var user = authstate.User;
            var ID = user.FindFirst(u => u.Type.Contains("nameidentifier"))?.Value;
            _athlete = await AthleteData.GetAthlete(ID);
            _vdotData[0] = _athlete.Vdot;
            _vdotData[1] = 85 - _athlete.Vdot;
            _isLoading = false;
            StateHasChanged();
            base.OnInitialized();
        }

        private async Task AddAthlete()
        {
            var options = new DialogOptions{CloseButton = true, DisableBackdropClick = false, MaxWidth = MaxWidth.Small};
            RaceResultModel res = null;
            if (_athlete.ActiveReferenceResult != null)
                res = _athlete.ActiveReferenceResult;
            var parameters = new DialogParameters<AddRaceDialog>{{x => x.Model, res}};
            var result = await dialogService.Show<AddRaceDialog>(string.Empty, parameters, options).Result;
            if (!result.Cancelled)
            {
                var rrm = result.Data as RaceResultModel ?? new RaceResultModel();
                _athlete.ActiveReferenceResult = rrm;
                _athlete.Vdot = vdotService.GetVdot(rrm);
                _vdotData[0] = _athlete.Vdot;
                _vdotData[1] = 85 - _athlete.Vdot; //ToDo: magic number entfernen
                _athlete.PaceModel = pmProvider[_athlete.Vdot];
                _isLoading = true;
                StateHasChanged();
                await AthleteData.UpdateAthlete(_athlete);
                _isLoading = false;
            }
        }
    }
}