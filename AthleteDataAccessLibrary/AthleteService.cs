using CoreLibrary.Models.Athlet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
