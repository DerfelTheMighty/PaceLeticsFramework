using PaceLetics.VdotModule.CodeBase.Models;

namespace PaceLetics.VdotModule.CodeBase.Interfaces
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