namespace PaceLetics.TrainingModule.CodeBase.Workouts.Models;

public sealed class ContentReference
{
    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;

    public ContentReference NormalizeCopy()
    {
        return new ContentReference
        {
            Title = Title?.Trim() ?? string.Empty,
            Url = Url?.Trim() ?? string.Empty,
            Description = Description?.Trim() ?? string.Empty,
            SourceType = SourceType?.Trim() ?? string.Empty
        };
    }

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Title)
        && string.IsNullOrWhiteSpace(Url)
        && string.IsNullOrWhiteSpace(Description)
        && string.IsNullOrWhiteSpace(SourceType);
}
