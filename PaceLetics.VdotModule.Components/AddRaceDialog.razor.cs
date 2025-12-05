using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using System;
using System.Text.RegularExpressions;
using MudBlazor;
using PaceLetics.VdotModule.CodeBase.Models;
using PaceLetics.CoreModule.Infrastructure.Constants;
using MudBlazor.Extensions.Core;
using MudBlazor.Extensions.Components;
using MudBlazor.Extensions;
using MudBlazor;

namespace PaceLetics.VdotModule.Components
{
    public partial class AddRaceDialog
    {
        private string _id = string.Empty;
        private DateTime? _date = DateTime.Now;
        private string _type = string.Empty;
        private long _distanceM = 0;
        private string _time = string.Empty;
        private IMask _timeMask = new PatternMask("00:00:00");
        private Regex _timePattern = new Regex(@"^(?:[01]\d|2[0-3]):(?:[0-5]\d):(?:[0-5]\d)$");
        [Parameter]
        public RaceResultModel Model { get; set; }

        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = default !;
        private void OK()
        {
            bool isValidTime = _timePattern.IsMatch(_time);
            if (isValidTime)
            {
                Model.Id = _id;
                Model.Date = _date ?? DateTime.Now;
                TimeSpan.TryParse(_time, out var res);
                Model.Time = res;
                Model.Type = _type;
                Model.DistanceM = _distanceM;
                this.MudDialog.Close(DialogResult.Ok(this.Model));
            }
            else
            {
                ShowTimeFormatError();
            }
        }

        private void OnRaceTypeChanged(string value)
        {
            _type = value;
            _distanceM = RaceDistances.Dict[value];
            StateHasChanged();
        }

        protected async override void OnInitialized()
        {
            if (Model == null)
            {
                Model = new RaceResultModel();
            }

            _id = Model.Id;
            _type = Model.Type;
            _date = Model.Date;
            _time = Model.Time.ToString();
            _distanceM = Model.DistanceM;
            StateHasChanged();
            base.OnInitialized();
        }

        private async Task ShowTimeFormatError()
        {
            int seconds = 3;
            IMudExDialogReference<MudExMessageDialog>? dlg = await dialogService.ShowInformationAsync("Achtung", $"Bitte überprüfe, ob die Eingabe der Laufzeit im Format hh:mm:ss vorliegt.", Icons.Material.Filled.Error, false, true);
            for (int i = 0; i < seconds; i++)
            {
                await Task.Delay(1000);
                dlg.ExecuteOnDialogComponent(dialog =>
                {
                    dialog.ProgressValue = (i + 1) * 100 / seconds;
                    dialog.Message = $"Bitte überprüfe, ob die Eingabe der Laufzeit im Format hh:mm:ss vorliegt.";
                });
            }

            dlg.Close();
        }
    }
}