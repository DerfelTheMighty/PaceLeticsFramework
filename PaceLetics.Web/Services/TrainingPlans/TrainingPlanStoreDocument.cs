using PaceLetics.CoreModule.Infrastructure.Interfaces;

namespace PaceLetics.Web.Services.TrainingPlans;

public sealed class TrainingPlanStoreDocument : IQueryItem
{
    public const string DocumentTypeValue = "trainingPlanDefinition";
    public const string PartitionKeyValue = "training-plans";

    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = PartitionKeyValue;
    public string DocumentType { get; set; } = DocumentTypeValue;
    public string Json { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
