using CoreLibrary.Models.Athlet;


namespace AthleteDataAccessLibrary.Contracts
{
    /// <summary>
    /// Provides the athletemodel for the active session
    /// </summary>
    public interface IAthleteService
    {
        /// <summary>
        /// Retuns the currently active Athletemodel
        /// </summary>
        /// <returns></returns>
        AthleteModel GetCurrentModel();

        /// <summary>
        /// Sets the current athlete model
        /// </summary>
        /// <returns></returns>
        void SetCurrentModel(AthleteModel model);

        /// <summary>
        /// Clears the currently active model
        /// </summary>
        /// <returns></returns>
        void ClearCurrentModel();

    }
}
