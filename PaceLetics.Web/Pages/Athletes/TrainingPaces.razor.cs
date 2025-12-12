using MudBlazor;
using MudBlazor.Extensions;
using PaceLetics.AthleteModule.CodeBase.Models;
using PaceLetics.CoreModule.Infrastructure.Models;


namespace PaceLetics.Web.Pages.Athletes
{
    public partial class TrainingPaces
    {
        private bool _isLoading = true;

        private AthleteModel? _athlete;
        private PaceModel _upperPace = new();
        private PaceModel _lowerPace = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var authState = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
                var userID = authState.User.FindFirst(u => u.Type.Contains("nameidentifier"))?.Value;

                if (!string.IsNullOrEmpty(userID))
                {
                    _athlete = await AthleteData.GetAthlete(userID);

                    if (_athlete?.PaceModel is not null)
                    {
                        _upperPace = _athlete.PaceModel;
                        _lowerPace = _athlete.PaceModel.Reduce(0.975);
                    }
                }
            }
            catch
            {
                await dialogService.ShowInformationAsync(
                    "Achtung",
                    "Es gab ein Problem beim Laden deiner Daten. Bitte versuche es später erneut.",
                    Icons.Material.Filled.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}