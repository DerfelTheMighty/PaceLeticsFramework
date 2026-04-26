
using PaceLetics.WorkoutModule.CodeBase.Enums;

namespace PaceLetics.WorkoutModule.CodeBase.Models
{
    public class DefinitionFactory
    {

        public List<ExerciseDefinition> CreateExerciseExamples() 
        {
            List<ExerciseDefinition> lst = new List<ExerciseDefinition>();

            #region glute bridge

            lst.Add(new ExerciseDefinition()
            {
                Id = "Glute Bridge Easy",
                Name = "Glute Bridge",
                Description = "Statische Übung für Hüftstreckung, Hüftstabilität und Abdruck",
                Execution = new List<string>()
                {
                    "Lege dich auf den Rücken und stelle die Beine an!",
                    "Drücke mit den Fußsohlen gegen den Boden!",
                    "Spanne deine Gesäßmuskulatur an und strecke so die Hüfte nach oben!",
                    "Setze Bauch- und Gesäßmuskulatur ein, um Streckung zu halten!"
                },
                Duration = 30,
                ImageFile = "glute_bridge_base.png",
                SwitchLeftRight = false,
                Level = Level.Easy
            }); 
            lst.Add(new ExerciseDefinition()
            {
                Id = "Glute Bridge Moderate",
                Name = "Walking Glute Bridge",
                Description = "Dynamische Übung für Hüftstreckung, Hüftstabilität und Abdruck",
                Execution = new List<string>()
                {
                    "Gehe in die Glute Bridge!",
                    "Spanne deine Gesäßmuskulatur an und strecke so die Hüfte nach oben!",
                    "Setze Bauch- und Gesäßmuskulatur ein, um Streckung zu halten!",
                    "Hebe die Füße abwechselnd kurz vom Boden ab!"
                },
                Duration = 60,
                ImageFile = "glute_bridge_waddle.gif",
                SwitchLeftRight = false,
                Level = Level.Moderate
            });
            lst.Add(new ExerciseDefinition()
            {
                Id = "Glute Bridge Advanced",
                Name = "Straight Leg Glute Bridge",
                Description = "Anspruchsvolle statische Übung für Hüftstreckung, Stabilität und Abdruck",
                Execution = new List<string>()
                {
                    "Gehe in die Glute Bridge!",
                    "Spanne deine Gesäßmuskulatur an und strecke so die Hüfte nach oben",
                    "Setze Bauch- und Gesäßmuskulatur ein, um Streckung zu halten",
                    "Strecke ein Bein aus und halte dabei die Hüftstreckung"
                },
                Duration = 30,
                ImageFile = "glute_bridge_straight_leg.png",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Advanced
            });
            lst.Add(new ExerciseDefinition()
            {
                Id = "Glute Bridge Epic",
                Name = "Glute Bridge Straight Leg Dip",
                Description = "Anspruchsvolle dynamische Übung für Hüftstreckung, -Stabilität und Abruck",
                Execution = new List<string>()
                {
                    "Gehe in die Straight Leg Glute Bridge!",
                    "Strecke ein Bein aus und halte dabei die Hüftstreckung",
                    "Senke die Hüfte bis kurz über dem Boden ab und hebe sie wieder an!",
                    "Setze Bauch- und Gesäßmuskulatur ein!"

                },
                Duration = 60,
                ImageFile = "glute_bridge_straight_leg_dip.gif",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Epic
            });

            #endregion

            #region quadruped

            lst.Add(new ExerciseDefinition()
            {
                Name = "Quadruped",
                Id = "Quadruped Easy",
                Description = "Statische Übung für Rumpfstabilität und Kräftigung des unteren Rückens.",
                Execution = new List<string>()
                {
                    "Gehe in den Vierfüßlerstand mit Oberkörper und Kopf auf einer Linie!",
                    "Strecke linken Arm und rechtes Bein aus!",
                    "Spanne Gesäß- und Bauchmuskulatur bewußt an!",
                    "Achte darauf, kein Hohlkreuz zu bilden!",
                },
                Duration = 30,
                ImageFile = "quadruped_base.png",
                SwitchLeftRight = true,
                SwitchTime=5,
                Level = Level.Easy
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Quadruped Crunsh",
                Id = "Quadruped Moderate",
                Description = "Dynamische Übung für Rumpfstabilität und funktionale Kräftigung des unteren Rückens.",
                Execution = new List<string>()
                {
                    "Gehe in den Vierfüßlerstand mit Oberkörper und Kopf auf einer Linie!",
                    "Strecke linken Arm und rechtes Bein aus!",
                    "Spanne Gesäß- und Bauchmuskulatur bewußt an!",
                    "Achte darauf, kein Hohlkreuz zu bilden!",
                    "Führe Ellenbogen und Knie unter dem Oberkörper zusammen und Strecke sie wieder aus!"
                },
                Duration = 45,
                ImageFile = "quadruped_crunsh.gif",
                SwitchLeftRight = true,
                SwitchTime=5,
                Level = Level.Moderate
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Quadruped Crawl",
                Id = "Quadruped Advanced",
                Description = "Anspruchsvolle Übung für Rumpfstabilität, Bauchmuskulatur und den unteren Rücken.",
                Execution = new List<string>()
                {
                    "Gehe in den Vierfüßlerstand mit Oberkörper und Kopf auf einer Linie!",
                    "Hebe die Knie vom Boden ab!",
                    "Spanne Gesäß- und Bauchmuskulatur bewußt an!",
                    "Achte darauf, kein Hohlkreuz zu bilden!",
                    "Hebe abwechselnd kurz die gegenüberliegenden Gliedmaßen vom Boden ab!"
                },
                Duration = 60,
                ImageFile = "quadruped_crawl.png",
                SwitchLeftRight = false,
                Level = Level.Advanced
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Quadruped Crawl Crunsh",
                Id = "Quadruped Epic",
                Description = "Anspruchsvolle Übung zur Kraftigung und Koordination im gesamten Rumpf.",
                Execution = new List<string>()
                {
                    "Gehe in den Vierfüßlerstand mit Oberkörper und Kopf auf einer Linie!",
                    "Hebe die Knie vom Boden ab!",
                    "Spanne Gesäß- und Bauchmuskulatur bewußt an!",
                    "Achte darauf, kein Hohlkreuz zu bilden!",
                    "Strecke abwechselnd die gegenüberliegenden Gliedmaßen aus!"
                },
                Duration = 45,
                ImageFile = "quadruped_crawl_crunsh.gif",
                SwitchLeftRight = true,
                Level = Level.Epic
            });
                
            lst.Add(new ExerciseDefinition()
            {
                Id = "Lateral Bounds",
                Name = "Lateral Bounds",
                Description = "Dynamische Übung für Fußarbeit, Reaktivkraft und Koordination.",
                Execution = new List<string>()
                {
                    "Gehe in eine leichte Grundposition mit gebeugten Knien!",
                    "Springe rhythmisch seitlich von einem Bein auf das andere!",
                    "Halte den Oberkörper stabil und lande weich auf dem Vorfuß!",
                    "Arbeite schnell und kontrolliert mit aktivem Armeinsatz!"
                },
                Duration = 30,
                ImageFile = "lateral_bounce.png",
                SwitchLeftRight = false,
                Level = Level.Epic
            });

            lst.Add(new ExerciseDefinition()
            {
                Id = "Cross Lunges",
                Name = "Cross-Lunges",
                Description = "Dynamische Übung für Beinachse, Gesäßmuskulatur und Hüftstabilität.",
                Execution = new List<string>()
                {
                    "Starte im aufrechten Stand!",
                    "Setze ein Bein diagonal hinter das Standbein in einen Crossover-Lunge!",
                    "Beuge beide Knie kontrolliert und halte den Oberkörper stabil!",
                    "Drücke dich kraftvoll zurück in den Stand und wechsle die Seite!"
                },
                Duration = 45,
                ImageFile = "cross_lunges.png",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Epic
            });

            lst.Add(new ExerciseDefinition()
            {
                Id = "Mountain Climber",
                Name = "Mountain Climber",
                Description = "Dynamische Übung für Core, Schulterstabilität und Ganzkörperausdauer.",
                Execution = new List<string>()
                {
                    "Gehe in den Streckstütz mit gestrecktem Körper!",
                    "Ziehe ein Knie schnell in Richtung Brust!",
                    "Setze den Fuß zurück und wechsle direkt die Seite!",
                    "Halte Bauch und Gesäß aktiv, damit die Hüfte ruhig bleibt!"
                },
                Duration = 45,
                ImageFile = "mountain_climber.png",
                SwitchLeftRight = false,
                Level = Level.Epic
            });

            lst.Add(new ExerciseDefinition()
            {
                Id = "Deep Squat Rotational Jumps",
                Name = "Deep Squat Rotational Jumps",
                Description = "Explosive Übung für Beinkraft, Koordination und Rotationsstabilität.",
                Execution = new List<string>()
                {
                    "Gehe in einen tiefen Squat!",
                    "Führe kleine Wechselhopser aus, sodass immer ein Fuß kurz frei ist!",
                    "Halte das Gesäß tief und richte dich möglichst nicht auf!",
                    "Arbeite kontrolliert aus der tiefen Position mit leichter Rotation!"
                },
                Duration = 45,
                ImageFile = "deep_squat_rotational_jumps.png",
                SwitchLeftRight = false,
                Level = Level.Epic
            });

            lst.Add(new ExerciseDefinition()
            {
                Id = "Deep Squat Jumps",
                Name = "Deep Squat Jumps",
                Description = "Explosive Übung für Beinkraft, Abdruck und Schnellkraft.",
                Execution = new List<string>()
                {
                    "Gehe in einen tiefen Squat!",
                    "Federe zweimal kurz in der tiefen Position nach!",
                    "Springe dann explosiv nach oben in die Streckung!",
                    "Lande weich und gehe direkt wieder kontrolliert tief!"
                },
                Duration = 45,
                ImageFile = "deep_jump_squat.png",
                SwitchLeftRight = false,
                Level = Level.Epic
            });

            lst.Add(new ExerciseDefinition()
            {
                Id = "Side Plank Hip Dips",
                Name = "Side Plank Hip Dips",
                Description = "Dynamische Übung für seitliche Rumpfstabilität und Hüftkontrolle.",
                Execution = new List<string>()
                {
                    "Gehe in den Seitstütz auf Unterarm oder Hand!",
                    "Halte den Körper in einer Linie!",
                    "Senke die Hüfte kontrolliert Richtung Boden ab!",
                    "Drücke die Hüfte wieder aktiv nach oben!"
                },
                Duration = 40,
                ImageFile = "side_plank_hip_dip.png",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Epic
            });

            lst.Add(new ExerciseDefinition()
            {
                Id = "Low Squat Jacks",
                Name = "Low Squat Jacks",
                Description = "Dynamische Übung für Beine, Gesäß und Kraftausdauer in tiefer Position.",
                Execution = new List<string>()
                {
                    "Gehe in eine tiefe Reiterstellung oder Squat-Position!",
                    "Bleibe tief mit aktivem Core!",
                    "Öffne und schließe die Beine schnell und kontrolliert!",
                    "Halte die Spannung und richte dich dabei nicht auf!"
                },
                Duration = 45,
                ImageFile = "low_squat_jack.png",
                SwitchLeftRight = false,
                Level = Level.Epic
            });

            lst.Add(new ExerciseDefinition()
            {
                Id = "Hand Release Push Up",
                Name = "Hand-Release Push-up",
                Description = "Kräftigende Übung für Brust, Schulter, Arme und Rumpf.",
                Execution = new List<string>()
                {
                    "Gehe in die Liegestützposition!",
                    "Senke den Körper kontrolliert bis zum Boden ab!",
                    "Löse die Hände kurz vom Boden!",
                    "Setze die Hände wieder auf und drücke dich kraftvoll hoch!"
                },
                Duration = 45,
                ImageFile = "hand_release_pushup.png",
                SwitchLeftRight = false,
                Level = Level.Epic
            });

            lst.Add(new ExerciseDefinition()
            {
                Id = "Alternating Squat Balance Hold",
                Name = "Alternating Squat Balance Hold",
                Description = "Anspruchsvolle Übung für Gleichgewicht, Beinachse und Hüftstabilität.",
                Execution = new List<string>()
                {
                    "Gehe in einen tiefen Squat!",
                    "Halte das Gesäß tief und den Oberkörper stabil!",
                    "Löse abwechselnd einen Fuß kurz vom Boden!",
                    "Bleibe kontrolliert in der tiefen Position und vermeide Ausweichbewegungen!"
                },
                Duration = 45,
                ImageFile = "alternating_squat_balance_hold.png",
                SwitchLeftRight = false,
                Level = Level.Epic
            });


            lst.Add(new ExerciseDefinition()
            {
                Id = "Burpees",
                Name = "Burpees",
                Description = "Ganzkörperübung für Schnellkraft, Koordination und Ausdauer.",
                Execution = new List<string>()
                {
                    "Starte im Stand!",
                    "Gehe tief, setze die Hände auf und springe oder steige in den Streckstütz!",
                    "Bringe die Füße zurück nach vorn!",
                    "Richte dich explosiv auf und springe nach oben ab!"
                },
                Duration = 45,
                ImageFile = "burpees.png",
                SwitchLeftRight = false,
                Level = Level.Epic
            });

            lst.Add(new ExerciseDefinition()
            {
                Id = "Lateral Step with Balance Lift",
                Name = "Lateral Step with Balance Lift",
                Description = "Koordinative Übung für Beinachse, Hüftstabilität und Gesäßmuskulatur.",
                Execution = new List<string>()
                {
                    "Gehe in eine tiefe seitliche Ausfallschrittposition!",
                    "Hebe das Standbein kurz kontrolliert vom Boden ab!",
                    "Bleibe tief und richte dich dabei nicht auf!",
                    "Wiederhole die Bewegung und wechsle danach die Seite!"
                },
                Duration = 45,
                ImageFile = "lateral_step_with_balance_lift.png",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Epic
            });

            lst.Add(new ExerciseDefinition()
            {
                Id = "Reverse Mountain Climber",
                Name = "Reverse Mountainclimber",
                Description = "Dynamische Übung für Core, Hüfte und Schulterstabilität in umgekehrter Stützposition.",
                Execution = new List<string>()
                {
                    "Gehe in den Rückstütz mit gehobener Hüfte!",
                    "Ziehe abwechselnd ein Knie Richtung Oberkörper!",
                    "Halte die Hüfte möglichst oben und stabil!",
                    "Arbeite kontrolliert und rhythmisch mit aktiver Körperspannung!"
                },
                Duration = 45,
                ImageFile = "reverse_mountain_climber.png",
                SwitchLeftRight = false,
                Level = Level.Epic
            });

            lst.Add(new ExerciseDefinition()
            {
                Id = "Horse Stance Overhead Swings",
                Name = "Horse Stance Overhead Swings",
                Description = "Dynamische Übung für Schultergürtel, Core und Kraftausdauer.",
                Execution = new List<string>()
                {
                    "Gehe in eine tiefe Reiterstellung!",
                    "Strecke die Arme über den Kopf!",
                    "Führe die Arme schnell rhythmisch vor und zurück!",
                    "Halte den Core angespannt und bleibe stabil in der tiefen Position!"
                },
                Duration = 45,
                ImageFile = "horse_stance_overhead_swings.png",
                SwitchLeftRight = false,
                Level = Level.Epic
            });

            lst.Add(new ExerciseDefinition()
            {
                Id = "Plank Rotation Alternating",
                Name = "Plank Rotation (alternating)",
                Description = "Anspruchsvolle Core-Übung für Schulterstabilität und kontrollierte Oberkörperrotation.",
                Execution = new List<string>()
                {
                    "Gehe in den Streckstütz!",
                    "Löse einen Arm und führe den Ellenbogen zum stützenden Arm!",
                    "Öffne den Oberkörper anschließend kontrolliert nach oben!",
                    "Führe den Ellenbogen nur bis senkrecht über den Körper und überdrehe nicht!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 40,
                ImageFile = "plank_rotation.png",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Epic
            });


            #endregion

            #region plank push up

            lst.Add(new ExerciseDefinition()
            {
                Name = "Plank push Up",
                Id = "Plank Push Up Easy",
                Description = "Effektive übung für den ganzen Rumpf",
                Execution = new List<string>()
                {
                    "Gehe in den Unterarmstütz (Plank) und spanne Gesäß- und Bauchmuskulatur bewußt an!",
                    "Setze, erst rechts, dann links, die Handfläche auf und drücke dich hoch in die Liegestützposition!",
                    "Gehe, erst rechts, dann links, wieder runter in die Plank position!",
                    "Wechsle die Reihenfolge beim Seitenwechsel!"
                },
                Duration = 15,
                ImageFile = "plank_push_up_base.gif",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Easy
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Plank push Up",
                Id = "Plank Push Up Moderate",
                Description = "Effektive übung für den ganzen Rumpf",
                Execution = new List<string>()
                {
                    "Gehe in den Unterarmstütz (Plank) und spanne Gesäß- und Bauchmuskulatur bewußt an!",
                    "Setze, erst rechts, dann links, die Handfläche auf und drücke dich hoch in die Liegestützposition!",
                    "Gehe, erst rechts, dann links, wieder runter in die Plank position!",
                    "Wechsle die Reihenfolge beim Seitenwechsel!"
                },
                Duration = 30,
                ImageFile = "plank_push_up_base.gif",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Moderate
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Plank push Up",
                Id = "Plank Push Up Advanced",
                Description = "Effektive übung für den ganzen Rumpf",
                Execution = new List<string>()
                {
                    "Gehe in den Unterarmstütz (Plank) und spanne Gesäß- und Bauchmuskulatur bewußt an!",
                    "Hebe das linke Bein vom Boden ab!",
                    "Setze, erst rechts, dann links, die Handfläche auf und drücke dich hoch in die Liegestützposition!",
                    "Gehe, erst rechts, dann links, wieder runter in die Plank position!",
                    "Wechsle die Reihenfolge beim Seitenwechsel!"
                },
                Duration = 30,
                ImageFile = "plank_push_up_leg_lift.gif",
                SwitchLeftRight = true,
                Level = Level.Advanced
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Plank push Up",
                Id = "Plank Push Up Epic",
                Description = "Effektive übung für den ganzen Rumpf",
                Execution = new List<string>()
                {
                    "Gehe in den Unterarmstütz (Plank) und spanne Gesäß- und Bauchmuskulatur bewußt an!",
                    "Hebe das linke Bein vom Boden ab!",
                    "Setze, erst rechts, dann links, die Handfläche auf und drücke dich hoch in die Liegestützposition!",
                    "Gehe, erst rechts, dann links, wieder runter in die Plank position!",
                    "Wechsle die Reihenfolge und das Standbein beim Seitenwechsel!"
                },
                Duration = 40,
                ImageFile = "plank_push_up_leg_lift.gif",
                SwitchLeftRight = true,
                Level = Level.Epic
            });

            #endregion

            #region side plank

            lst.Add(new ExerciseDefinition()
            {
                Name = "Side Plank",
                Id = "Side Plank Easy",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter.",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante.",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Nach Abschluss, wechsle zur anderen Seite."
                },
                Duration = 20,
                ImageFile = "side_plank_base.png",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Easy
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Side Plank",
                Id = "Side Plank Moderate",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter.",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante.",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Bewege deine Hüfte kontrolliert auf und ab. Halte die Grundposition stabil!",
                    "Nach Abschluss, wechsle zur anderen Seite."
                },
                Duration = 30,
                ImageFile = "side_plank_dip.gif",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Moderate
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Side Plank",
                Id = "Side Plank Advanced",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter.",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante.",
                    "Hebe das obere Bein vom Boden ab und halte es oben!",
                    "Nach Abschluss, wechsle zur anderen Seite."

                },
                Duration = 30,
                ImageFile = "side_plank_leg_raise.png",
                SwitchLeftRight = true,
                Level = Level.Advanced
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Side Plank",
                Id = "Side Plank Epic",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter.",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante.",
                    "Hebe das obere Bein und bewege die Hüfte kontrolliert auf und ab, halte die Grundposition stabil!",
                    "Nach Abschluss, wechsle zur anderen Seite."
                },
                Duration = 45,
                ImageFile = "side_plank_leg_raise_dip.gif",
                SwitchLeftRight = true,
                Level = Level.Epic
            });


            #endregion

            #region Plank dip


            lst.Add(new ExerciseDefinition()
            {
                Name = "Plank Dip",
                Id = "Plank Dip Easy",
                Description = "Statische Übung für Stabilität im Rumpf.",
                Execution = new List<string>()
                {
                    "Gehe in den Unterarmstütz (Plank) mit Gewicht auf den Unterarmen und Fußballen.",
                    "Spanne dabei Gesäß- und Bauchmuskulatur bewußt an!",
                    "Achte darauf, den Kopf in Verlängerung der Wirbelsäule zu halten, lass ihn nicht hängen!",
                    "Arbeite mit deiner Gesäß- und Bauchmuskulatur, um die Position stabil zu halten."
                },
                Duration = 30,
                ImageFile = "plank_base.png",
                SwitchLeftRight = false,
                SwitchTime = 5,
                Level = Level.Easy
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Plank Dip",
                Id = "Plank Dip Moderate",
                Description = "Statische Übung für Stabilität im Rumpf.",
                Execution = new List<string>()
                {
                    "Gehe in den Unterarmstütz (Plank) mit Gewicht auf den Unterarmen und Fußballen.",
                    "Spanne dabei Gesäß- und Bauchmuskulatur bewußt an!",
                    "Achte darauf, den Kopf in Verlängerung der Wirbelsäule zu halten, lass ihn nicht hängen!",
                    "Arbeite mit deiner Gesäß- und Bauchmuskulatur, um die Position stabil zu halten."
                },
                Duration = 60,
                ImageFile = "plank_base.png",
                SwitchLeftRight = false,
                SwitchTime = 5,
                Level = Level.Moderate
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Plank Dip",
                Id = "Plank Dip Advanced",
                Description = "Dymische Übung für Stabilität und Kraft im Rumpf.",
                Execution = new List<string>()
                {
                    "Gehe in den Unterarmstütz (Plank). Spanne dabei Gesäß- und Bauchmuskulatur bewußt an!",
                    "Achte darauf, den Kopf in Verlängerung der Wirbelsäule zu halten, lass ihn nicht hängen!",
                    "Arbeite mit deiner Gesäß- und Bauchmuskulatur, um die Position stabil zu halten.",
                    "Tippe mit der Hüfte abwechselnd nach links und rechts sanft in Richtung Boden."
                },
                Duration = 60,
                ImageFile = "plank_dip.gif",
                SwitchLeftRight = false,
                Level = Level.Advanced
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Plank Dip",
                Id = "Plank Dip Epic",
                Description = "Dymische Übung für Stabilität und Kraft im Rumpf.",
                Execution = new List<string>()
                {
                    "Gehe in den Unterarmstütz (Plank). Spanne dabei Gesäß- und Bauchmuskulatur bewußt an!",
                    "Achte darauf, den Kopf in Verlängerung der Wirbelsäule zu halten, lass ihn nicht hängen!",
                    "Arbeite mit deiner Gesäß- und Bauchmuskulatur, um die Position stabil zu halten.",
                    "Tippe mit der Hüfte abwechselnd nach links und rechts sanft in Richtung Boden."
                },
                Duration = 90,
                ImageFile = "plank_dip.gif",
                SwitchLeftRight = false,
                Level = Level.Epic
            });

            #endregion

            #region one leg deadlift

            lst.Add(new ExerciseDefinition()
            {
                Name = "One Leg Deadlift",
                Id = "One Leg Deadlift Easy",
                Description = "Effektive Übung für Balance und Rumpf- und Hüftstabilität!",
                Execution = new List<string>()
                {
                    "Starte stehend, spanne Gesäß-, Bauch- und Rückenmuskulatur an; Oberkörper bildet eine gerade Linie!",
                    "Gewicht auf ein Bein verlagern, während das andere Bein nach hinten gestreckt wird!",
                    "Kippe den Oberkörper nach vorne, sodass er parallel zum Boden ist, Arme hängen Richtung boden!",
                    "Führe die Übung langsam und kontrolliert aus, um Gleichgewicht und Stabilität zu wahren!",
                },
                Duration = 30,
                ImageFile = "one_leg_deadlift_hang.gif",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Easy
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "One Leg Deadlift",
                Id = "One Leg Deadlift Moderate",
                Description = "Effektive Übung für Balance und Rumpf- und Hüftstabilität!",
                Execution = new List<string>()
                {
                    "Starte stehend, spanne Gesäß-, Bauch- und Rückenmuskulatur an; Oberkörper bildet eine gerade Linie!",
                    "Gewicht auf ein Bein verlagern, während das andere Bein nach hinten gestreckt wird!",
                    "Kippe den Oberkörper nach vorne, sodass er parallel zum Boden ist, Arme bleiben durchgängig gestreckt!",
                    "Führe die Übung langsam und kontrolliert aus, um Gleichgewicht und Stabilität zu wahren!",
                },
                Duration = 45,
                ImageFile = "one_leg_deadlift_hang.gif",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Moderate
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "One Leg Deadlift",
                Id = "One Leg Deadlift Advanced",
                Description = "Effektive Übung für Balance und Rumpf- und Hüftstabilität!",
                Execution = new List<string>()
                {
                    "Starte stehend, Arme nach oben, spanne Gesäß-, Bauch- und Rückenmuskulatur an; Körper bildet eine gerade Linie!",
                    "Gewicht auf ein Bein verlagern, während das andere Bein nach hinten gestreckt wird!",
                    "Kippe den Oberkörper nach vorne, sodass er parallel zum Boden ist, Arme bleiben durchgängig gestreckt!",
                    "Führe die Übung langsam und kontrolliert aus, um Gleichgewicht und Stabilität zu wahren!",

                },
                Duration = 60,
                ImageFile = "one_leg_deadlift_straight.gif",
                SwitchLeftRight = true,
                Level = Level.Advanced
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "One Leg Deadlift",
                Id = "One Leg Deadlift Epic",
                Description = "Effektive Übung für Balance und Rumpf- und Hüftstabilität!",
                Execution = new List<string>()
                {
                    "Starte stehend, Arme nach oben, spanne Gesäß-, Bauch- und Rückenmuskulatur an; Körper bildet eine gerade Linie!",
                    "Gewicht auf ein Bein verlagern, während das andere Bein nach hinten gestreckt wird!",
                    "Kippe den Oberkörper nach vorne, sodass er parallel zum Boden ist, Arme bleiben durchgängig gestreckt!",
                    "Führe die Übung langsam und kontrolliert aus, um Gleichgewicht und Stabilität zu wahren.",
                },
                Duration = 90,
                ImageFile = "one_leg_deadlift_straight.gif",
                SwitchLeftRight = true,
                Level = Level.Epic
            });

            #endregion

            #region superwoman

            lst.Add(new ExerciseDefinition()
            {
                Name = "Superwoman",
                Id = "Superwoman Easy",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich flach auf den Bauch, Arme nach oben gestreckt. Der ganze Körper bildet eine Linie.",
                    "Konzentriere das Gewicht auf deine Bauchpartie. Hebe Arme und Beine leicht vom Boden ab.",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Arbeite bewusst mit deiner Gesäß- und Bauchmuskulatur, während du die Position hältst!"
                },
                Duration = 30,
                ImageFile = "superwoman_base.png",
                SwitchLeftRight = false,
                SwitchTime = 5,
                Level = Level.Easy
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Superwoman",
                Id = "Superwoman Moderate",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich flach auf den Bauch, Arme nach oben gestreckt.",
                    "Konzentriere das Gewicht auf deine Bauchpartie. Hebe Arme und Beine leicht vom Boden ab.",
                    "Arbeite bewusst mit deiner Gesäß- und Bauchmuskulatur! Der ganze Körper bildet eine Linie!",
                    "Hebe wechselseit diagonal jeweils einen Arm und ein Bein etwas höher."
                },
                Duration = 30,
                ImageFile = "superwoman_paddle.gif",
                SwitchLeftRight = false,
                SwitchTime = 5,
                Level = Level.Moderate
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Superwoman",
                Id = "Superwoman Advanced",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich flach auf den Bauch, Arme nach oben gestreckt.",
                    "Konzentriere das Gewicht auf deine Bauchpartie. Hebe Arme und Beine leicht vom Boden ab.",
                    "Arbeite bewusst mit deiner Gesäß- und Bauchmuskulatur! Der ganze Körper bildet eine Linie!",
                    "Hebe wechselseit diagonal jeweils einen Arm und ein Bein etwas höher."
                },
                Duration = 60,
                ImageFile = "superwoman_paddle.gif",
                SwitchLeftRight = false,
                Level = Level.Advanced
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Superwoman",
                Id = "Superwoman Epic",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich flach auf den Bauch, Arme nach oben gestreckt.",
                    "Konzentriere das Gewicht auf deine Bauchpartie. Hebe Arme und Beine leicht vom Boden ab.",
                    "Arbeite bewusst mit deiner Gesäß- und Bauchmuskulatur! Der ganze Körper bildet eine Linie!",
                    "Hebe wechselseit diagonal jeweils einen Arm und ein Bein etwas höher."
                },
                Duration = 90,
                ImageFile = "superwoman_paddle.gif",
                SwitchLeftRight = false,
                Level = Level.Epic
            });

            #endregion

            #region mountain climber
            lst.Add(new ExerciseDefinition()
            {
                Name = "Mountain Climber",
                Id = "Mountain Climber Easy",
                Description = "Effektive Übung für Kraft und Koordination im gesamten Rumpf.",
                Execution = new List<string>()
                {
                    "Gehe in die Liegestützposition!",
                    "Halte den Kopf in einer Linie mit dem Rücken, Blickrichtung zum Boden!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Führe abwechselnd die Knie langsam in Richtung der gleichseitigen Ellenbogen!"
                },
                Duration = 30,
                ImageFile = "mountain_climber_same.gif",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Easy
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Crossover Mountain Climber",
                Id = "Mountain Climber Moderate",
                Description = "Effektive Übung für Kraft und Koordination im gesamten Rumpf.",
                Execution = new List<string>()
                {
                    "Gehe in die Liegestützposition!",
                    "Halte den Kopf in einer Linie mit dem Rücken, Blickrichtung zum Boden!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Führe abwechselnd die Knie langsam in Richtung der gleichseitigen Ellenbogen!"
                },
                Duration = 30,
                ImageFile = "mountain_climber_crossover.gif",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Moderate
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Mountain Climber",
                Id = "Mountain Climber Advanced",
                Description = "Effektive Übung für Kraft und Koordination im gesamten Rumpf.",
                Execution = new List<string>()
                {
                    "Gehe in die Liegestützposition!",
                    "Halte den Kopf in einer Linie mit dem Rücken, Blickrichtung zum Boden!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Führe abwechselnd die Knie langsam in Richtung der gegenüberliegenden Ellenbogen!"

                },
                Duration = 60,
                ImageFile = "mountain_climber_same.gif",
                SwitchLeftRight = true,
                Level = Level.Advanced
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Fast Crossover Mountain Climber",
                Id = "Mountain Climber Epic",
                Description = "Effektive Übung für Kraft und Koordination im gesamten Rumpf.",
                Execution = new List<string>()
                {
                    "Gehe in die Liegestützposition!",
                    "Halte den Kopf in einer Linie mit dem Rücken, Blickrichtung zum Boden!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Führe abwechselnd das Knie schnell in Richtung des gegenüberliegenden Ellenbogen!"
                },
                Duration = 60,
                ImageFile = "mountain_climber_crossover.gif",
                SwitchLeftRight = true,
                Level = Level.Epic
            });

            #endregion


            return lst;
        }


        public List<WorkoutDefinition> CreateWorkoutExamples() 
        {
            List<WorkoutDefinition> lst = new List<WorkoutDefinition>();
            lst.Add(new WorkoutDefinition()
            {
                Name = "Stabi Handout",
                Id = "Stabi Handout Easy",
                Description = "Das Basisprogramm für den Rumpf",
                SwitchTime = 5,
                PreparationTime = 10,
                RestTime = 10,
                Level = Enums.Level.Easy,
                Exercises= new List<string>() 
                {
                    "Glute Bridge Easy",
                    "Quadruped Easy",
                    "Plank Dip Easy",
                    "Side Plank Easy",
                    "Plank Push Up Easy",
                    "One Leg Deadlift Easy",
                    "Superwoman Easy",
                    "Mountain Climber Easy"
                }
            });
            lst.Add(new WorkoutDefinition()
            {
                Name = "Stabi Handout",
                Id = "Stabi Handout Moderate",
                Description = "Das Basisprogramm für den Rumpf",
                SwitchTime = 5,
                PreparationTime = 10,
                RestTime = 10,
                Level = Enums.Level.Moderate,
                Exercises = new List<string>()
                {
                    "Glute Bridge Moderate",
                    "Quadruped Moderate",
                    "Plank Dip Moderate",
                    "Side Plank Moderate",
                    "Plank Push Up Moderate",
                    "One Leg Deadlift Moderate",
                    "Superwoman Moderate",
                    "Mountain Climber Moderate"
                }
            });
            lst.Add(new WorkoutDefinition()
            {
                Name = "Stabi Handout",
                Id = "Stabi Handout Advanced",
                Description = "Das Basisprogramm für den Rumpf",
                SwitchTime = 5,
                PreparationTime = 10,
                RestTime = 10,
                Level = Enums.Level.Advanced,
                Exercises = new List<string>()
                {
                    "Glute Bridge Advanced",
                    "Quadruped Advanced",
                    "Plank Dip Advanced",
                    "Side Plank Advanced",
                    "Plank Push Up Advanced",
                    "One Leg Deadlift Advanced",
                    "Superwoman Advanced",
                    "Mountain Climber Advanced"
                }
            });

            lst.Add(new WorkoutDefinition()
            {
                Name = "Stabi Handout",
                Id = "Stabi Handout Epic",
                Description = "Das Basisprogramm für den Rumpf",
                SwitchTime = 5,
                PreparationTime = 10,
                RestTime = 10,
                Level = Enums.Level.Epic,
                Exercises = new List<string>()
                {
                    "Glute Bridge Epic",
                    "Quadruped Epic",
                    "Plank Dip Epic",
                    "Side Plank Epic",
                    "Plank Push Up Epic",
                    "One Leg Deadlift Epic",
                    "Superwoman Epic",
                    "Mountain Climber Epic" }
            });

            lst.Add(new WorkoutDefinition()
            {
                Name = "Race Optimization by Explosive Rebound Training Overload",
                Id = "roberto_circle",
                Description = "Explosives Ganzkörper-Workout für Kraft, Stabilität und Reaktivität.",
                SwitchTime = 5,
                PreparationTime = 10,
                RestTime = 10,
                Level = Enums.Level.Epic,
                Exercises = new List<string>()
    {
        "Lateral Bounds",
        "Cross Lunges",
        "Mountain Climber",
        "Deep Squat Rotational Jumps",
        "Deep Squat Jumps",
        "Side Plank Hip Dips",
        "Low Squat Jacks",
        "Hand Release Push Up",
        "Alternating Squat Balance Hold",
        "Burpees",
        "Lateral Step with Balance Lift",
        "Reverse Mountain Climber",
        "Horse Stance Overhead Swings",
        "Plank Rotation Alternating"
    }});

            return lst;

        }

    }
}
