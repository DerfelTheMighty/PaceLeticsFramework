using System.Text.RegularExpressions;
using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;
using PaceLetics.TrainingModule.CodeBase.Workouts.Repositories;

namespace PaceLetics.Web.Services.Workouts;

public sealed class WorkoutCatalogManagementService
{
    private readonly IWorkoutCatalogStore _store;
    private readonly IWorkoutCatalogValidator _validator;
    private readonly WorkoutCatalogDocument _catalog;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _isLoaded;

    public WorkoutCatalogManagementService(
        IWorkoutCatalogStore store,
        IWorkoutCatalogValidator validator,
        WorkoutCatalogDocument catalog)
    {
        _store = store;
        _validator = validator;
        _catalog = catalog;
    }

    public WorkoutCatalogDocument Catalog => _catalog;

    public async Task EnsureLoadedAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_isLoaded)
                return;

            var loadedCatalog = await _store.LoadOrSeedAsync(CloneCatalog(_catalog));
            _validator.NormalizeAndValidate(loadedCatalog);
            Apply(loadedCatalog);
            _isLoaded = true;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task UpsertExerciseAsync(ExerciseDefinition exercise, string? trainerUserId)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        await EnsureLoadedAsync();
        await _gate.WaitAsync();
        try
        {
            var candidate = CloneCatalog(_catalog);
            var now = DateTime.UtcNow;
            var ownerUserId = NormalizeOwner(trainerUserId);
            var normalized = CloneExercise(exercise);
            normalized.Id = NormalizeId(normalized.Id, normalized.Name);
            normalized.Name = normalized.Name?.Trim() ?? string.Empty;
            normalized.Description = normalized.Description?.Trim() ?? string.Empty;
            normalized.ImageFile = normalized.ImageFile?.Trim() ?? string.Empty;
            normalized.Source = normalized.Source?.Trim() ?? string.Empty;
            normalized.OwnerUserId = string.IsNullOrWhiteSpace(normalized.OwnerUserId)
                ? ownerUserId
                : normalized.OwnerUserId.Trim();
            normalized.CreatedAt ??= now;
            normalized.UpdatedAt = now;

            var index = candidate.Exercises.FindIndex(existing =>
                string.Equals(existing.Id, normalized.Id, StringComparison.OrdinalIgnoreCase)
                && existing.Level == normalized.Level);
            if (index >= 0)
            {
                normalized.CreatedAt = candidate.Exercises[index].CreatedAt ?? normalized.CreatedAt;
                candidate.Exercises[index] = normalized;
            }
            else
            {
                candidate.Exercises.Add(normalized);
            }

            await SaveAndApplyAsync(candidate);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task UpsertWorkoutAsync(WorkoutDefinition workout, string? trainerUserId)
    {
        ArgumentNullException.ThrowIfNull(workout);

        await EnsureLoadedAsync();
        await _gate.WaitAsync();
        try
        {
            var candidate = CloneCatalog(_catalog);
            var now = DateTime.UtcNow;
            var ownerUserId = NormalizeOwner(trainerUserId);
            var normalized = CloneWorkout(workout);
            normalized.Id = NormalizeId(normalized.Id, normalized.Name);
            normalized.Name = normalized.Name?.Trim() ?? string.Empty;
            normalized.Description = normalized.Description?.Trim() ?? string.Empty;
            normalized.Source = normalized.Source?.Trim() ?? string.Empty;
            normalized.OwnerUserId = string.IsNullOrWhiteSpace(normalized.OwnerUserId)
                ? ownerUserId
                : normalized.OwnerUserId.Trim();
            normalized.CreatedAt ??= now;
            normalized.UpdatedAt = now;

            var index = candidate.Workouts.FindIndex(existing =>
                string.Equals(existing.Id, normalized.Id, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                normalized.CreatedAt = candidate.Workouts[index].CreatedAt ?? normalized.CreatedAt;
                candidate.Workouts[index] = normalized;
            }
            else
            {
                candidate.Workouts.Add(normalized);
            }

            await SaveAndApplyAsync(candidate);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task SaveAndApplyAsync(WorkoutCatalogDocument candidate)
    {
        _validator.NormalizeAndValidate(candidate);
        await _store.SaveAsync(candidate);
        Apply(candidate);
        _isLoaded = true;
    }

    private void Apply(WorkoutCatalogDocument candidate)
    {
        _catalog.SchemaVersion = candidate.SchemaVersion;
        Replace(_catalog.Exercises, candidate.Exercises);
        Replace(_catalog.Workouts, candidate.Workouts);
    }

    private static void Replace<T>(List<T> target, IEnumerable<T> source)
    {
        target.Clear();
        target.AddRange(source);
    }

    private static WorkoutCatalogDocument CloneCatalog(WorkoutCatalogDocument source)
    {
        return new WorkoutCatalogDocument
        {
            SchemaVersion = source.SchemaVersion,
            Exercises = source.Exercises.Select(CloneExercise).ToList(),
            Workouts = source.Workouts.Select(CloneWorkout).ToList()
        };
    }

    private static ExerciseDefinition CloneExercise(ExerciseDefinition source)
    {
        return new ExerciseDefinition
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            Execution = source.Execution?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList() ?? new(),
            Duration = source.Duration,
            ImageFile = source.ImageFile,
            Level = source.Level,
            SwitchLeftRight = source.SwitchLeftRight,
            SwitchTime = source.SwitchTime,
            Tags = source.Tags?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList() ?? new(),
            ReadMore = source.ReadMore?.Select(CloneReference).Where(x => !x.IsEmpty).ToList() ?? new(),
            Source = source.Source,
            OwnerUserId = source.OwnerUserId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    private static WorkoutDefinition CloneWorkout(WorkoutDefinition source)
    {
        return new WorkoutDefinition
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            Level = source.Level,
            PreparationTime = source.PreparationTime,
            RestTime = source.RestTime,
            SwitchTime = source.SwitchTime,
            Exercises = source.Exercises?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList() ?? new(),
            Tags = source.Tags?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList() ?? new(),
            ReadMore = source.ReadMore?.Select(CloneReference).Where(x => !x.IsEmpty).ToList() ?? new(),
            Source = source.Source,
            OwnerUserId = source.OwnerUserId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    private static ContentReference CloneReference(ContentReference source)
    {
        return new ContentReference
        {
            Title = source.Title,
            Url = source.Url,
            Description = source.Description,
            SourceType = source.SourceType
        }.NormalizeCopy();
    }

    private static string NormalizeOwner(string? trainerUserId)
    {
        return string.IsNullOrWhiteSpace(trainerUserId) ? "trainer" : trainerUserId.Trim();
    }

    private static string NormalizeId(string? id, string? fallbackName)
    {
        if (!string.IsNullOrWhiteSpace(id))
            return id.Trim();

        var fallback = string.IsNullOrWhiteSpace(fallbackName) ? "workout-item" : fallbackName.Trim().ToLowerInvariant();
        return Regex.Replace(fallback, "[^a-z0-9]+", "-", RegexOptions.CultureInvariant).Trim('-');
    }
}
