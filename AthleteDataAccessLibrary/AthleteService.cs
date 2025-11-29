using AthleteDataAccessLibrary.Contracts;
using PaceLetics.AthleteModule.CodeBase.Models;

namespace AthleteDataAccessLibrary
{

    public class AthleteService : IAthleteService
    {
        private AthleteModel? _model;

        public void ClearCurrentModel()
        {
            _model = null;
        }

        public AthleteModel GetCurrentModel() 
        {
            if (_model == null)
                return null;
            return _model;
        }

        public void SetCurrentModel(AthleteModel model)
        {
            _model = model;
        }

    }

}
