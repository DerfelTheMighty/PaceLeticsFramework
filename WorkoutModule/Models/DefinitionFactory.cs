using WorkoutModule.Enums;
namespace WorkoutModule.Models
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
                Description = "Übung für Hüftstreckung, Hüftstabilität und Abdruck",
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
                Description = "Übung für Hüftstreckung, Hüftstabilität und Abdruck",
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
                Description = "Übung für Hüftstreckung, Stabilität und Abdruck",
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
                Description = "Übung für Hüftstreckung, -Stabilität und Abruck",
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
                Description = "Übung für Körperspannung und Hüftstabilität",
                Execution = new List<string>()
                {
                    "Gehe in den Vierfüßlerstand mit Oberkörper und Kopf auf einer Linie!",
                    "Strecke linken Arm und rechtes Bein aus!",
                    "Spanne Gesäß- und Bauchmuskulatur bewußt an!",
                    "Achte darauf, kein Hohlkreuz zu bilden!",
                },
                Duration = 10,
                ImageFile = "quadruped_base.png",
                SwitchLeftRight = true,
                SwitchTime=5,
                Level = Level.Easy
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Quadruped",
                Id = "Quadruped Moderate",
                Description = "Übung für Körperspannung und Hüftstabilität",
                Execution = new List<string>()
                {
                    "Gehe in den Vierfüßlerstand mit Oberkörper und Kopf auf einer Linie!",
                    "Strecke linken Arm und rechtes Bein aus!",
                    "Spanne Gesäß- und Bauchmuskulatur bewußt an!",
                    "Achte darauf, kein Hohlkreuz zu bilden!",
                    "Führe Ellenbogen und Knie unter dem Oberkörper zusammen und Strecke sie wieder aus!"
                },
                Duration = 60,
                ImageFile = "quadruped_stretch.png",
                SwitchLeftRight = true,
                SwitchTime=5,
                Level = Level.Moderate
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Quadruped",
                Id = "Quadruped Advanced",
                Description = "Übung für Körperspannung und Hüftstabilität",
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
                Name = "Quadruped",
                Id = "Quadruped Epic",
                Description = "Übung für Körperspannung und Hüftstabilität",
                Execution = new List<string>()
                {
                    "Gehe in den Vierfüßlerstand mit Oberkörper und Kopf auf einer Linie!",
                    "Hebe die Knie vom Boden ab!",
                    "Spanne Gesäß- und Bauchmuskulatur bewußt an!",
                    "Achte darauf, kein Hohlkreuz zu bilden!",
                    "Strecke abwechselnd die gegenüberliegenden Gliedmaßen aus!"
                },
                Duration = 60,
                ImageFile = "quadruped_crawl_stretch.png",
                SwitchLeftRight = false,
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
                Duration = 30,
                ImageFile = "plank_push_up_base.png",
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
                Duration = 60,
                ImageFile = "plank_push_up_base.png",
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
                    "Setze, erst rechts, dann links, die Handfläche auf und drücke dich hoch in die Liegestützposition!",
                    "Gehe, erst rechts, dann links, wieder runter in die Plank position!",
                    "Wechsle die Reihenfolge beim Seitenwechsel!"
                },
                Duration = 30,
                ImageFile = "plank_push_up_single_leg.png",
                SwitchLeftRight = false,
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
                    "Hebe das rechte Bein vom Boden ab.",
                    "Setze, erst rechts, dann links, die Handfläche auf und drücke dich hoch in die Liegestützposition!",
                    "Gehe, erst rechts, dann links, wieder runter in die Plank position!",
                    "Wechsle die Reihenfolge und das Standbein beim Seitenwechsel!"
                },
                Duration = 60,
                ImageFile = "plank_push_up_single_leg.png",
                SwitchLeftRight = false,
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
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 30,
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
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Senke und hebe deine Hüfte kontrolliert nach unten und oben! Halte die Grundposition stabil!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 60,
                ImageFile = "side_plank_dip.png",
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
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Hebe das oberes Bein vom Boden ab und halte es oben.",
                    "Wechsle anschließend die Seite!"
                    
                },
                Duration = 30,
                ImageFile = "side_plank_single_leg.png",
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
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Hebe das oberes Bein vom Boden ab und halte es oben.",
                    "Senke und hebe deine Hüfte kontrolliert nach unten und oben! Halte die Grundposition stabil!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 60,
                ImageFile = "side_plank_single_leg_dip.png",
                SwitchLeftRight = true,
                Level = Level.Epic
            });


            #endregion

            #region Plank dip


            lst.Add(new ExerciseDefinition()
            {
                Name = "Plank Dip",
                Id = "Plank Dip Easy",
                Description = "Effektive Übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 30,
                ImageFile = "plank_dip_base.png",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Easy
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Plank Dip",
                Id = "Plank Dip Moderate",
                Description = "Effektive Übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Senke und hebe deine Hüfte kontrolliert nach unten und oben! Halte die Grundposition stabil!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 60,
                ImageFile = "plank_dip_base.png",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Moderate
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Plank Dip",
                Id = "Plank Dip Advanced",
                Description = "Effektive Übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Hebe das oberes Bein vom Boden ab und halte es oben.",
                    "Wechsle anschließend die Seite!"

                },
                Duration = 30,
                ImageFile = "plank_dip_base.png",
                SwitchLeftRight = true,
                Level = Level.Advanced
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Plank Dip",
                Id = "Plank Dip Epic",
                Description = "Effektive Übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Hebe das oberes Bein vom Boden ab und halte es oben.",
                    "Senke und hebe deine Hüfte kontrolliert nach unten und oben! Halte die Grundposition stabil!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 60,
                ImageFile = "plank_dip_base.png",
                SwitchLeftRight = true,
                Level = Level.Epic
            });

            #endregion

            #region one leg deadlift

            lst.Add(new ExerciseDefinition()
            {
                Name = "One Leg Deadlift",
                Id = "One Leg Deadlift Easy",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 30,
                ImageFile = "one_leg_deadlift_base.png",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Easy
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "One Leg Deadlift",
                Id = "One Leg Deadlift Moderate",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Senke und hebe deine Hüfte kontrolliert nach unten und oben! Halte die Grundposition stabil!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 60,
                ImageFile = "one_leg_deadlift_base.png",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Moderate
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "One Leg Deadlift",
                Id = "One Leg Deadlift Advanced",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Hebe das oberes Bein vom Boden ab und halte es oben.",
                    "Wechsle anschließend die Seite!"

                },
                Duration = 30,
                ImageFile = "one_leg_deadlift_base.png",
                SwitchLeftRight = true,
                Level = Level.Advanced
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "One Leg Deadlift",
                Id = "One Leg Deadlift Epic",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Hebe das oberes Bein vom Boden ab und halte es oben.",
                    "Senke und hebe deine Hüfte kontrolliert nach unten und oben! Halte die Grundposition stabil!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 60,
                ImageFile = "one_leg_deadlift_base.png",
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
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 30,
                ImageFile = "superwoman_base.png",
                SwitchLeftRight = true,
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
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Senke und hebe deine Hüfte kontrolliert nach unten und oben! Halte die Grundposition stabil!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 60,
                ImageFile = "superwoman_base.png",
                SwitchLeftRight = true,
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
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Hebe das oberes Bein vom Boden ab und halte es oben.",
                    "Wechsle anschließend die Seite!"

                },
                Duration = 30,
                ImageFile = "superwoman_base.png",
                SwitchLeftRight = true,
                Level = Level.Advanced
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Superwoman",
                Id = "Superwoman Epic",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Hebe das oberes Bein vom Boden ab und halte es oben.",
                    "Senke und hebe deine Hüfte kontrolliert nach unten und oben! Halte die Grundposition stabil!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 60,
                ImageFile = "superwoman_base.png",
                SwitchLeftRight = true,
                Level = Level.Epic
            });

            #endregion

            #region mountain climber
            lst.Add(new ExerciseDefinition()
            {
                Name = "Mountain Climber",
                Id = "Mountain Climber Easy",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 30,
                ImageFile = "mountain_climber_base.png",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Easy
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Mountain Climber",
                Id = "Mountain Climber Moderate",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Senke und hebe deine Hüfte kontrolliert nach unten und oben! Halte die Grundposition stabil!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 60,
                ImageFile = "mountain_climber_base.png",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Moderate
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Mountain Climber",
                Id = "Mountain Climber Advanced",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Hebe das oberes Bein vom Boden ab und halte es oben.",
                    "Wechsle anschließend die Seite!"

                },
                Duration = 30,
                ImageFile = "mountain_climber_base.png",
                SwitchLeftRight = true,
                Level = Level.Advanced
            });

            lst.Add(new ExerciseDefinition()
            {
                Name = "Mountain Climber",
                Id = "Mountain Climber Epic",
                Description = "Effektive übung für die Beinachse und die seitliche Rumpfmuskulatur",
                Execution = new List<string>()
                {
                    "Lege dich seitlich auf den linken Arm, Ellenbogen unter der Schulter!",
                    "Hebe deine Körpermitte vom Boden ab und stütze dich dabei auf Ellenbogen und Fußkante!",
                    "Achte darauf, dass der Körper eine gerade Linie bildet!",
                    "Hebe das oberes Bein vom Boden ab und halte es oben.",
                    "Senke und hebe deine Hüfte kontrolliert nach unten und oben! Halte die Grundposition stabil!",
                    "Wechsle anschließend die Seite!"
                },
                Duration = 60,
                ImageFile = "mountain_climber_base.png",
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
                Description = "Unser Einstiegsprogramm für den Rumpf",
                SwitchTime = 5,
                PreparationTime = 10,
                RestTime = 10,
                Level = Enums.Level.Easy,
                Exercises= new List<string>() 
                {
                    "Glute Bridge Easy",
                    "Quadruped Easy",
                    "Plank Push Up Easy",
                    "Side Plank Easy",
                    "Plank Dip Easy",
                    "One Leg Deadlift Easy",
                    "Superwoman Easy",
                    "Mountain Climber Easy"
                }
            });
            lst.Add(new WorkoutDefinition()
            {
                Name = "Stabi Handout",
                Id = "Stabi Handout Moderate",
                Description = "Das solide Basisprogramm für den Rumpf",
                SwitchTime = 5,
                PreparationTime = 10,
                RestTime = 10,
                Level = Enums.Level.Moderate,
                Exercises = new List<string>()
                {
                    "Glute Bridge Moderate",
                    "Quadruped Moderate",
                    "Plank Push Up Moderate",
                    "Side Plank Moderate",
                    "Plank Dip Moderate",
                    "One Leg Deadlift Moderate",
                    "Superwoman Moderate",
                    "Mountain Climber Moderate"
                }
            });
            lst.Add(new WorkoutDefinition()
            {
                Name = "Stabi Handout",
                Id = "Stabi Handout Advanced",
                Description = "Ambitioniertes Rumpftraining für ambitionierte Läufer:innen",
                SwitchTime = 5,
                PreparationTime = 10,
                RestTime = 10,
                Level = Enums.Level.Advanced,
                Exercises = new List<string>()
                {
                    "Glute Bridge Advanced",
                    "Quadruped Advanced",
                    "Plank Push Up Advanced",
                    "Side Plank Advanced",
                    "Plank Dip Advanced",
                    "One Leg Deadlift Advanced",
                    "Superwoman Advanced",
                    "Mountain Climber Advanced"
                }
            });

            lst.Add(new WorkoutDefinition()
            {
                Name = "Stabi Handout",
                Id = "Stabi Handout Epic",
                Description = "Rumpftraining aus der Hölle",
                SwitchTime = 5,
                PreparationTime = 10,
                RestTime = 10,
                Level = Enums.Level.Epic,
                Exercises = new List<string>()
                {
                    "Glute Bridge Epic",
                    "Quadruped Epic",
                    "Plank Push Up Epic",
                    "Side Plank Epic",
                    "Plank Dip Epic",
                    "One Leg Deadlift Epic",
                    "Superwoman Epic",
                    "Mountain Climber Epic" }
            });
            return lst;

        }

    }
}
