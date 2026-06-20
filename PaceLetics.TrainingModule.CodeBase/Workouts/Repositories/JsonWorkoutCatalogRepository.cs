using System.Text.Json;
using System.Text.Json.Serialization;
using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Repositories
{
    public sealed class JsonWorkoutCatalogRepository : IWorkoutCatalogRepository, IWorkoutCatalogValidator
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
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

            NormalizeAndValidate(document);
            return document;
        }

        public void Save(WorkoutCatalogDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            NormalizeAndValidate(document);

            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(document, Options);
            File.WriteAllText(_filePath, json);
        }

        public void NormalizeAndValidate(WorkoutCatalogDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            Normalize(document);
            Validate(document);
        }

        private static void Normalize(WorkoutCatalogDocument document)
        {
            foreach (var exercise in document.Exercises)
            {
                exercise.Tags = NormalizeTags(exercise.Tags);
                exercise.ReadMore = NormalizeReferences(exercise.ReadMore);
                exercise.Source = exercise.Source?.Trim() ?? string.Empty;
                exercise.OwnerUserId = exercise.OwnerUserId?.Trim() ?? string.Empty;
            }

            foreach (var workout in document.Workouts)
            {
                workout.Tags = NormalizeTags(workout.Tags);
                workout.ReadMore = NormalizeReferences(workout.ReadMore);
                workout.Source = workout.Source?.Trim() ?? string.Empty;
                workout.OwnerUserId = workout.OwnerUserId?.Trim() ?? string.Empty;
            }
        }

        private static List<string> NormalizeTags(IEnumerable<string>? tags)
        {
            return (tags ?? Enumerable.Empty<string>())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<ContentReference> NormalizeReferences(IEnumerable<ContentReference>? references)
        {
            return (references ?? Enumerable.Empty<ContentReference>())
                .Where(reference => reference is not null)
                .Select(reference => reference.NormalizeCopy())
                .ToList();
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

                ValidateReferences(
                    exercise.ReadMore,
                    $"Exercise '{exercise.Id}'",
                    errors);
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

                ValidateReferences(
                    workout.ReadMore,
                    $"Workout '{workout.Id}'",
                    errors);
            }

            if (errors.Count > 0)
            {
                throw new WorkoutCatalogValidationException(
                    $"Workout catalog '{_filePath}' is invalid.",
                    errors);
            }
        }

        private static void ValidateReferences(
            IReadOnlyList<ContentReference> references,
            string owner,
            List<string> errors)
        {
            foreach (var reference in references)
            {
                if (reference.IsEmpty)
                {
                    errors.Add($"{owner} contains an empty readMore reference.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(reference.Url))
                    continue;

                if (!Uri.TryCreate(reference.Url, UriKind.RelativeOrAbsolute, out _))
                    errors.Add($"{owner} contains invalid readMore url '{reference.Url}'.");
            }
        }
    }
}
