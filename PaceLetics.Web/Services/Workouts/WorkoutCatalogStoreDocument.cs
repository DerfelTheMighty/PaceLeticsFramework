using PaceLetics.CoreModule.Infrastructure.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.Web.Services.Workouts;

public static class WorkoutCatalogStoreDocumentTypes
{
    public const string Catalog = "workoutCatalog";
}

public sealed class WorkoutCatalogStoreDocument : IQueryItem
{
    public string Id { get; set; } = WorkoutCatalogStoreDocumentIds.Catalog("de");
    public string CourseId { get; set; } = WorkoutCatalogStoreDocumentIds.Partition("de");
    public string DocumentType { get; set; } = WorkoutCatalogStoreDocumentTypes.Catalog;
    public string Locale { get; set; } = "de";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public WorkoutCatalogDocument Catalog { get; set; } = new();

    public void Normalize(string locale)
    {
        locale = string.IsNullOrWhiteSpace(locale) ? "de" : locale.Trim().ToLowerInvariant();
        Id = WorkoutCatalogStoreDocumentIds.Catalog(locale);
        CourseId = WorkoutCatalogStoreDocumentIds.Partition(locale);
        DocumentType = WorkoutCatalogStoreDocumentTypes.Catalog;
        Locale = locale;
        Catalog ??= new WorkoutCatalogDocument();
    }

    public static WorkoutCatalogStoreDocument Create(string locale, WorkoutCatalogDocument catalog, DateTime now)
    {
        var document = new WorkoutCatalogStoreDocument
        {
            Catalog = catalog,
            CreatedAt = now,
            UpdatedAt = now
        };
        document.Normalize(locale);
        return document;
    }
}

public static class WorkoutCatalogStoreDocumentIds
{
    public static string Catalog(string locale)
    {
        return $"workout-catalog:{NormalizeLocale(locale)}";
    }

    public static string Partition(string locale)
    {
        return Catalog(locale);
    }

    private static string NormalizeLocale(string locale)
    {
        return string.IsNullOrWhiteSpace(locale) ? "de" : locale.Trim().ToLowerInvariant();
    }
}
