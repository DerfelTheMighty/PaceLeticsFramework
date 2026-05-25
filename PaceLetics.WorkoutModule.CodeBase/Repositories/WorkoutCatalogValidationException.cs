namespace PaceLetics.WorkoutModule.CodeBase.Repositories
{
    public sealed class WorkoutCatalogValidationException : Exception
    {
        public WorkoutCatalogValidationException(string message, IReadOnlyList<string> errors)
            : base(message)
        {
            Errors = errors;
        }

        public IReadOnlyList<string> Errors { get; }
    }
}
