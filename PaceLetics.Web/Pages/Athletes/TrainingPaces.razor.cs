using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Extensions.Components;
using MudBlazor.Extensions.Core;
using PaceLetics.AthleteModule.CodeBase.Models;
using PaceLetics.CoreModule.Infrastructure.Constants;
using PaceLetics.VdotModule.CodeBase.Models;

namespace PaceLetics.Web.Pages.Athletes
{
    public partial class TrainingPaces
    {
        private bool _isLoading = true;
        private AthleteModel? _athlete;
        private PaceModel _lowerPace = new PaceModel();
        private PaceModel _upperPace = new PaceModel();
        private int _selectedPaceItem;
        private ElementReference _paceinforef;
        protected override async Task OnInitializedAsync()
        {
            try
            {
                var authState = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
                var userID = authState.User.FindFirst(u => u.Type.Contains("nameidentifier"))?.Value;
                if (!string.IsNullOrEmpty(userID))
                {
                    _athlete = await AthleteData.GetAthlete(userID);
                    if (_athlete?.PaceModel != null)
                    {
                        _upperPace = _athlete.PaceModel;
                        _lowerPace = _athlete.PaceModel.Reduce(0.975);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "$Es gab ein Problem beim Laden Ihrer Daten. Bitte versuchen Sie es später erneut oder kontaktieren Sie den Support.";
                IMudExDialogReference<MudExMessageDialog>? dlg = await dialogService.ShowInformationAsync("Achtung", errorMessage, Icons.Material.Filled.Error, false, true);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task OnShowMoreInfo(string key)
        {
            var item = GetInfoItem(key);
            _selectedPaceItem = item;
            StateHasChanged();
            await JSRuntime.InvokeVoidAsync("scrollToTop");
        }

        private int GetInfoItem(string type)
        {
            int infoItem = 0;
            switch (type)
            {
                case PaceKeys.Easy:
                    infoItem = 0;
                    break;
                case PaceKeys.Marathon:
                    infoItem = 1;
                    break;
                case PaceKeys.Threshold:
                    infoItem = 2;
                    break;
                case PaceKeys.Intervall:
                    infoItem = 3;
                    break;
                case PaceKeys.Repetition:
                    infoItem = 4;
                    break;
                default:
                    infoItem = 0;
                    break;
            }

            return infoItem;
        }
    }
}