namespace PaceLetics.CoreModule.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface garauantess item to be queryiable from a database
    /// </summary>
    public interface IQueryItem
    {
        /// <summary>
        /// Item identfier
        /// </summary>
        string Id { get; set; }
    }

    /// <summary>
    /// Marks documents whose writes must fail when another request changed the document
    /// after it was read.
    /// </summary>
    public interface IVersionedQueryItem : IQueryItem
    {
        string? ETag { get; set; }
    }

    public sealed class OptimisticConcurrencyException : Exception
    {
        public OptimisticConcurrencyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
