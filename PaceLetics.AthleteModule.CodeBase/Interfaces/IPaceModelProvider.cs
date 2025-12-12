using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.AthleteModule.CodeBase.Interfaces
{
    public interface IPaceModelProvider
    {
        PaceModel this[double vdot] { get; }

        int Count { get; }
        List<PaceModel> Registry { get; set; }
        ICollection<PaceModel> Values { get; }

        void Clear();
        string ToString();
    }
}