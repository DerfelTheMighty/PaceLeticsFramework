using AthleteDataAccessLibrary;
using AthleteDataAccessLibrary.Contracts;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.Web.Services.Workouts;

public sealed class CosmosWorkoutCatalogStore : IWorkoutCatalogStore
{
    private const string Locale = "de";

    private readonly IDataAccess _db;
    private readonly AthleteDataOptions _options;
    private readonly TimeProvider _timeProvider;

    public CosmosWorkoutCatalogStore(IDataAccess db, AthleteDataOptions options, TimeProvider? timeProvider = null)
    {
        _db = db;
        _options = options;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _options.Validate();
    }

    public async Task<WorkoutCatalogDocument> LoadOrSeedAsync(WorkoutCatalogDocument seedCatalog)
    {
        ArgumentNullException.ThrowIfNull(seedCatalog);

        var document = await LoadDocumentAsync();
        if (document is not null)
        {
            document.Normalize(Locale);
            return document.Catalog;
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        document = WorkoutCatalogStoreDocument.Create(Locale, seedCatalog, now);
        await SaveDocumentAsync(document);
        return document.Catalog;
    }

    public async Task SaveAsync(WorkoutCatalogDocument catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var existing = await LoadDocumentAsync();
        var document = WorkoutCatalogStoreDocument.Create(Locale, catalog, now);
        document.CreatedAt = existing?.CreatedAt ?? now;
        document.UpdatedAt = now;

        await SaveDocumentAsync(document);
    }

    private Task<WorkoutCatalogStoreDocument?> LoadDocumentAsync()
    {
        return _db.LoadItem<WorkoutCatalogStoreDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            WorkoutCatalogStoreDocumentIds.Catalog(Locale),
            WorkoutCatalogStoreDocumentIds.Partition(Locale));
    }

    private Task SaveDocumentAsync(WorkoutCatalogStoreDocument document)
    {
        document.Normalize(Locale);

        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            document,
            document.CourseId);
    }
}
