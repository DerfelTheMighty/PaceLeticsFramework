using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.Web.Services.Courses;

public static class CourseSeedData
{
    private static readonly string[] LevelOnePlanIds =
    {
        "Level 1 Laufschule 2.5k Track A",
        "Level 1 Laufschule 2.5k Track B",
        "Level 1 Laufschule 5k Track C",
        "Level 1 Laufschule 5k Track D",
        "Level 1 Laufschule 10k Track E"
    };

    public static IReadOnlyList<CourseDocument> CreateDefaultCourses()
    {
        return new[]
        {
            CreateCourse(
                "laufschule-einsteigende",
                "Laufschule für Einsteigende",
                "Level 1",
                "Grundlagenkurs für den sicheren Einstieg ins strukturierte Lauftraining.",
                new DateTime(2026, 4, 29),
                new DateTime(2026, 6, 24),
                LevelOnePlanIds,
                CreateDates(
                    "laufschule-einsteigende",
                    "Laufschule",
                    new[]
                    {
                        new DateTime(2026, 4, 29),
                        new DateTime(2026, 5, 6),
                        new DateTime(2026, 5, 13),
                        new DateTime(2026, 5, 20),
                        new DateTime(2026, 5, 27),
                        new DateTime(2026, 6, 3),
                        new DateTime(2026, 6, 10),
                        new DateTime(2026, 6, 17),
                        new DateTime(2026, 6, 24)
                    },
                    new TimeSpan(18, 0, 0),
                    TimeSpan.FromMinutes(90)),
                new[] { CreateChristophOCourseLead() }),
            CreateCourse(
                "technik-schnelligkeit-level-2",
                "Schnelligkeits- und Techniktraining Level 2",
                "Level 2",
                "Technik, Laufökonomie und Schnelligkeitsentwicklung für fortgeschrittene Einsteiger:innen.",
                new DateTime(2026, 4, 16),
                new DateTime(2026, 7, 16),
                new[] { "Level 2 Technik & Schnelligkeit" },
                CreateDates(
                    "technik-schnelligkeit-level-2",
                    "Techniktraining Level 2",
                    TechniqueCourseDates,
                    new TimeSpan(18, 30, 0),
                    TimeSpan.FromMinutes(90)),
                new[] { CreateChristophOCourseLead() }),
            CreateCourse(
                "technik-schnelligkeit-level-3",
                "Schnelligkeits- und Techniktraining Level 3",
                "Level 3",
                "Anspruchsvolle Einheiten für Athlet:innen mit stabiler Trainingsbasis.",
                new DateTime(2026, 4, 16),
                new DateTime(2026, 7, 16),
                new[] { "Level 3 Technik & Schnelligkeit" },
                CreateDates(
                    "technik-schnelligkeit-level-3",
                    "Techniktraining Level 3",
                    TechniqueCourseDates,
                    new TimeSpan(19, 0, 0),
                    TimeSpan.FromMinutes(90)),
                new[] { CreateChristophOCourseLead() })
        };
    }

    private static readonly DateTime[] TechniqueCourseDates =
    {
        new(2026, 4, 16),
        new(2026, 4, 23),
        new(2026, 4, 30),
        new(2026, 5, 7),
        new(2026, 5, 21),
        new(2026, 5, 28),
        new(2026, 6, 4),
        new(2026, 6, 11),
        new(2026, 6, 14),
        new(2026, 6, 18),
        new(2026, 6, 25),
        new(2026, 7, 2),
        new(2026, 7, 9),
        new(2026, 7, 16)
    };

    private static CourseDocument CreateCourse(
        string id,
        string name,
        string level,
        string description,
        DateTime startDate,
        DateTime endDate,
        IEnumerable<string> trainingPlanIds,
        IEnumerable<CourseDateDocument> dates,
        IEnumerable<CourseTrainerDocument>? trainers = null)
    {
        var trainerList = trainers?.ToList() ?? new List<CourseTrainerDocument>();

        return new CourseDocument
        {
            Id = id,
            CourseId = id,
            Slug = id,
            Name = name,
            TeamId = id,
            Level = level,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            CreatedByTrainerUserId = trainerList.FirstOrDefault()?.TrainerUserId ?? string.Empty,
            Dates = dates.ToList(),
            Trainers = trainerList,
            TrainingPlanPublications = trainingPlanIds
                .Select(planId => new CourseTrainingPlanPublicationDocument
                {
                    TrainingPlanId = planId,
                    PublishedAt = startDate.AddDays(-14),
                    VisibleFrom = startDate,
                    Target = FeedTarget.Course(id)
                })
                .ToList()
        };
    }

    private static CourseTrainerDocument CreateChristophOCourseLead()
    {
        return new CourseTrainerDocument
        {
            TrainerUserId = "0d14d8e3-755a-46be-81fa-de3f15d64812",
            DisplayName = "ChristophO",
            Role = "Kursleitung",
            CanManagePlans = true,
            CanManageEvents = true,
            CanManageMembers = true
        };
    }

    private static IEnumerable<CourseDateDocument> CreateDates(
        string courseId,
        string title,
        IEnumerable<DateTime> dates,
        TimeSpan startsAt,
        TimeSpan duration)
    {
        return dates.Select((date, index) => new CourseDateDocument
        {
            Id = $"{courseId}-{index + 1:00}",
            Title = title,
            StartsAt = date.Date.Add(startsAt),
            EndsAt = date.Date.Add(startsAt).Add(duration)
        });
    }
}
