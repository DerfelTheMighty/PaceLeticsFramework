using WorkoutModule.Enums;
namespace WorkoutModule.Models
{
    public class DefinitionFactory
    {

        public List<ExerciseDefinition> CreateExerciseExamples() 
        {
            List<ExerciseDefinition> lst = new List<ExerciseDefinition>();

            lst.Add(new ExerciseDefinition()
            {
                Id = "Glute Bridge Easy",
                Description = "Übung für Hüftstreckung, Stabilität und Abruck",
                Execution = new List<string>()
                {
                    "Lege dich auf den Rücken und stelle die Beine an!",
                    "Drücke mit den Fußsohlen gegen den Boden!",
                    "Spanne deine Gesäßmuskulatur an und strecke so die Hüfte nach oben!",
                    "Setze Bauch- und Gesäßmuskulatur ein, um Streckung zu halten!"
                },
                Duration = 10,
                ImageFile = "glute_bridge_base.png",
                SwitchLeftRight = false,
                Level = Level.Easy
            }); 
            lst.Add(new ExerciseDefinition()
            {
                Id = "Glute Bridge Moderate",
                Description = "Übung für Hüftstreckung, Stabilität und Abruck",
                Execution = new List<string>()
                {
                    "Lege dich auf den Rücken und stelle die Beine an!",
                    "Drücke mit den Fußsohlen gegen den Boden!",
                    "Spanne deine Gesäßmuskulatur an und strecke so die Hüfte nach oben!",
                    "Setze Bauch- und Gesäßmuskulatur ein, um Streckung zu halten!",
                    "Hebe die Füße abwechselnd kurz vom Boden ab!"
                },
                Duration = 60,
                ImageFile = "glute_bridge_marching.png",
                SwitchLeftRight = false,
                Level = Level.Moderate
            });
            lst.Add(new ExerciseDefinition()
            {
                Id = "Glute Bridge Advanced",
                Description = "Übung für Hüftstreckung, Stabilität und Abruck",
                Execution = new List<string>()
                {
                    "Lege dich auf den Rücken und stelle die Beine an",
                    "Drücke mit den Fußsohlen gegen den Boden",
                    "Spanne deine Gesäßmuskulatur an und strecke so die Hüfte nach oben",
                    "Setze Bauch- und Gesäßmuskulatur ein, um Streckung zu halten",
                    "Strecke ein Bein aus und halte dabei die Hüftstreckung"
                },
                Duration = 30,
                ImageFile = "glute_bridge_single.png",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Advanced
            });
            lst.Add(new ExerciseDefinition()
            {
                Id = "Glute Bridge Epic",
                Description = "Übung für Hüftstreckung, -Stabilität und Abruck",
                Execution = new List<string>()
                {
                    "Lege dich auf den Rücken und stelle die Beine an",
                    "Drücke mit den Fußsohlen gegen den Boden",
                    "Spanne deine Gesäßmuskulatur an und strecke so die Hüfte nach oben",
                    "Setze Bauch- und Gesäßmuskulatur ein, um Streckung zu halten",
                    "Strecke ein Bein aus und halte dabei die Hüftstreckung"

                },
                Duration = 30,
                ImageFile = "glute_bridge_single_raise.png",
                SwitchLeftRight = true,
                SwitchTime = 5,
                Level = Level.Epic
            });

            lst.Add(new ExerciseDefinition()
            {
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

            return lst;


        }


        public WorkoutDefinition CreateStabiHandoutExample() 
        {
            WorkoutDefinition workoutDef = new WorkoutDefinition()
            {
                Id = "Stabi Handout Easy",
                Description = "Basisprogramm für den Rumpf",
                SwitchTime = 5,
                PreparationTime = 10,
                RestTime = 10,
                Level = Enums.Level.Easy,
                Exercises= new List<string>() 
                {
                    "Glute Bridge Easy",
                    "Quadruped Easy"
                }
            };

            return workoutDef;

        }

    }
}
