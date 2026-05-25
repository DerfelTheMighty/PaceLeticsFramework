using System.Text.Json;
using System.Text.Json.Serialization;
using PaceLetics.WorkoutModule.CodeBase.Enums;
using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Repositories
{
    public sealed class JsonWorkoutCatalogRepository : IWorkoutCatalogRepository
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        private readonly string _filePath;

        public JsonWorkoutCatalogRepository(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Catalog file path must not be empty.", nameof(filePath));

            _filePath = filePath;
        }

        public WorkoutCatalogDocument Load()
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException("Workout catalog file was not found.", _filePath);

            var json = File.ReadAllText(_filePath);
            var document = JsonSerializer.Deserialize<WorkoutCatalogDocument>(json, Options)
                ?? throw new InvalidDataException($"Workout catalog '{_filePath}' is empty or invalid.");

            Validate(document);
            return document;
        }

        private void Validate(WorkoutCatalogDocument document)
        {
            var errors = new List<string>();

            if (document.SchemaVersion != 1)
                errors.Add($"Unsupported schemaVersion '{document.SchemaVersion}'. Expected schemaVersion '1'.");

            if (document.Exercises.Count == 0)
                errors.Add("Catalog must contain at least one exercise.");

            if (document.Workouts.Count == 0)
                errors.Add("Catalog must contain at least one workout.");

            var exerciseKeys = new HashSet<(string Id, Level Level)>();
            foreach (var exercise in document.Exercises)
            {
                if (string.IsNullOrWhiteSpace(exercise.Id))
                    errors.Add("Exercise id must not be empty.");

                if (exercise.Level == Level.None)
                    errors.Add($"Exercise '{exercise.Id}' must define a level.");

                if (exercise.Duration < 0)
                    errors.Add($"Exercise '{exercise.Id}' duration must not be negative.");

                if (exercise.SwitchTime < 0)
                    errors.Add($"Exercise '{exercise.Id}' switchTime must not be negative.");

                if (!string.IsNullOrWhiteSpace(exercise.Id)
                    && !exerciseKeys.Add((exercise.Id, exercise.Level)))
                {
                    errors.Add($"Duplicate exercise id/level combination '{exercise.Id}'/'{exercise.Level}'.");
                }
            }

            var workoutIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var workout in document.Workouts)
            {
                if (string.IsNullOrWhiteSpace(workout.Id))
                    errors.Add("Workout id must not be empty.");
                else if (!workoutIds.Add(workout.Id))
                    errors.Add($"Duplicate workout id '{workout.Id}'.");

                if (string.IsNullOrWhiteSpace(workout.Name))
                    errors.Add($"Workout '{workout.Id}' name must not be empty.");

                if (workout.Level == Level.None)
                    errors.Add($"Workout '{workout.Id}' must define a level.");

                if (workout.PreparationTime < 0)
                    errors.Add($"Workout '{workout.Id}' preparationTime must not be negative.");

                if (workout.RestTime < 0)
                    errors.Add($"Workout '{workout.Id}' restTime must not be negative.");

                if (workout.SwitchTime < 0)
                    errors.Add($"Workout '{workout.Id}' switchTime must not be negative.");

                if (workout.Exercises.Count == 0)
                    errors.Add($"Workout '{workout.Id}' must contain at least one exercise.");

                foreach (var exerciseId in workout.Exercises)
                {
                    if (string.IsNullOrWhiteSpace(exerciseId))
                    {
                        errors.Add($"Workout '{workout.Id}' contains an empty exercise reference.");
                        continue;
                    }

                    if (!exerciseKeys.Contains((exerciseId, workout.Level)))
                    {
                        errors.Add(
                            $"Workout '{workout.Id}' references missing exercise '{exerciseId}' for level '{workout.Level}'.");
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new WorkoutCatalogValidationException(
                    $"Workout catalog '{_filePath}' is invalid.",
                    errors);
            }
        }
    }
}
