namespace CoreLibrary.Contracts
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
}
