using CoreLibrary.Models.Pace;

namespace VdotModule.Contracts;

public interface IPaceModelProvider
{
    PaceModel this[double vdot] { get; }

    int Count { get; }
    List<PaceModel> Registry { get; set; }
    ICollection<PaceModel> Values { get; }

    void Clear();
    string ToString();
}