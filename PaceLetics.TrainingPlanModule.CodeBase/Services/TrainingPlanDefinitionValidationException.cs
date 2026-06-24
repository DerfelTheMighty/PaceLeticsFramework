namespace PaceLetics.TrainingPlanModule.CodeBase.Services;

public sealed class TrainingPlanDefinitionValidationException : Exception
{
    public TrainingPlanDefinitionValidationException(string message, IReadOnlyList<string> errors)
        : base(message)
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}
