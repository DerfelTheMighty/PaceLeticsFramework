using Microsoft.AspNetCore.Components;


namespace PaceLetics.VdotModule.Components
{
    public partial class PaceInfo
    {
        public record PaceInfoItem(
         string Title,
         string Icon,
         TimeSpan Upper,
         TimeSpan Lower,
         string Description
     );

        [Parameter] public TimeSpan EPaceLow { get; set; }
        [Parameter] public TimeSpan EPaceHigh { get; set; }

        [Parameter] public TimeSpan MPaceLow { get; set; }
        [Parameter] public TimeSpan MPaceHigh { get; set; }

        [Parameter] public TimeSpan TPaceLow { get; set; }
        [Parameter] public TimeSpan TPaceHigh { get; set; }

        [Parameter] public TimeSpan IPaceLow { get; set; }
        [Parameter] public TimeSpan IPaceHigh { get; set; }

        [Parameter] public TimeSpan RPaceLow { get; set; }
        [Parameter] public TimeSpan RPaceHigh { get; set; }

        private List<PaceInfoItem> Items;

        protected override void OnParametersSet()
        {
            Items = new()
        {
            new("E Pace - Grundlagenausdauer", "/images/icons/epace.png",
                Upper: EPaceHigh, Lower: EPaceLow,
                Description: EText),

            new("M Pace - Marathon", "/images/icons/mpace.png",
                Upper: MPaceHigh, Lower: MPaceLow,
                Description: MText),

            new("T Pace - Schwelle", "/images/icons/tpace.png",
                Upper: TPaceHigh, Lower: TPaceLow,
                Description: TText),

            new("I Pace - VO2max", "/images/icons/ipace.png",
                Upper: IPaceHigh, Lower: IPaceLow,
                Description: IText),

            new("R Pace - Repetition", "/images/icons/rpace.png",
                Upper: RPaceHigh, Lower: RPaceLow,
                Description: RText)
        };
        }


 
        private string EText = @"Die E-Pace bildet das Fundament deiner aeroben Ausdauer. 
Sie liegt deutlich unter der Schwelle und wird zu einem großen Teil über den Fettstoffwechsel getragen. 
Die meisten deiner Wochenkilometer sollten in diesem Tempo stattfinden. 
Die angegebene Geschwindigkeit beschreibt den oberen GA1-Bereich.";

        private string MText = @"Die M-Pace entspricht deinem realistischen Marathonrenntempo. 
Sie ist überwiegend aerob, aber kohlenhydratintensiver als E-Pace. 
Viele empfinden hier einen angenehmen Flow. 
Für reines Grundlagentraining ist sie aber viel zu schnell.";

        private string TText = @"Die T-Pace liegt nahe deinem 10-km-Tempo und verbindet hohe aerobe und anaerobe Anteile. 
Dieses Tempo kannst du ungefähr eine Stunde halten. 
Fortgeschrittene nutzen es für längere Intervalle, Einsteigende auch für kürzere Abschnitte.";

        private string IText = @"Die I-Pace liegt deutlich oberhalb der Schwelle und setzt starke Reize auf deine VO2max. 
Intervalle sind kurz, die Pausen zu kurz für eine komplette Erholung.";

        private string RText = @"Die R-Pace ist sehr schnell und technisch anspruchsvoll. 
Sie verbessert Laufökonomie, Kraft und Koordination. 
Intervalle sind kurz, Pausen lang. 
Vor allem für erfahrene Läufer:innen geeignet.";
    }
}
