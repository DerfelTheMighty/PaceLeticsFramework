using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;
using MudBlazor;
using PaceLetics.CoreModule.Infrastructure.Constants;
using PaceLetics.CoreModule.Infrastructure.Models;


namespace PaceLetics.AthleteModule.Components
{
    public partial class AddRaceDialog
    {
        private string _id = string.Empty;
        private DateTime? _date = DateTime.Now;
        private string _type = string.Empty;
        private long _distanceM = 0;
        private string _time = string.Empty;
        private readonly IMask _timeMask = new PatternMask("00:00:00");
        private static readonly Regex TimePattern = new(@"^(?:[01]\d|2[0-3]):(?:[0-5]\d):(?:[0-5]\d)$");
        private static readonly string[] RaceTypes =
        [
            RaceKeys.D1k,
            RaceKeys.D1200m,
            RaceKeys.D3k,
            RaceKeys.D3600m,
            RaceKeys.D5k,
            RaceKeys.D10k,
            RaceKeys.D15k,
            RaceKeys.D21k,
            RaceKeys.D42k
        ];
        private bool IsValidTime => TimePattern.IsMatch(_time);
        private bool CanSubmit => !string.IsNullOrWhiteSpace(_id)
            && !string.IsNullOrWhiteSpace(_type)
            && _date.HasValue
            && IsValidTime;
        [Parameter]
        public RaceResultModel Model { get; set; } = new();

        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = default !;
        private void OK()
        {
            if (!CanSubmit)
                return;

            Model.Id = _id.Trim();
            Model.Date = _date!.Value;
            TimeSpan.TryParse(_time, out var res);
            Model.Time = res;
            Model.Type = _type;
            Model.DistanceM = _distanceM;
            MudDialog.Close(DialogResult.Ok(Model));
        }

        private void Cancel() => MudDialog.Cancel();

        private void OnRaceTypeChanged(string? value)
        {
            _type = value ?? string.Empty;
            _distanceM = RaceDistances.Dict.TryGetValue(_type, out var distance) ? distance : 0;
        }

        protected override void OnInitialized()
        {
            if (Model == null)
            {
                Model = new RaceResultModel();
            }

            _id = Model.Id ?? string.Empty;
            _type = Model.Type ?? string.Empty;
            _date = Model.Date;
            _time = Model.Time == TimeSpan.Zero ? string.Empty : Model.Time.ToString(@"hh\:mm\:ss");
            _distanceM = Model.DistanceM;
            base.OnInitialized();
        }

    }
}
