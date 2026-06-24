using PaceLetics.TrainingModule.CodeBase.Running.Definitions;
using PaceLetics.TrainingModule.CodeBase.Running.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Running.Models;

namespace PaceLetics.TrainingModule.CodeBase.Running.Services;

public sealed class RunningSessionFactory : IRunningSessionFactory
{
    public RunningSession Create(RunningSessionDefinition definition)
    {
        return definition switch
        {
            IntervalSessionDefinition interval => CreateInterval(interval),
            PlannedSessionDefinition planned => CreatePlanned(planned),
            _ => throw new InvalidDataException($"Unknown session definition type: {definition.GetType().Name}")
        };
    }

    public IReadOnlyList<RunningSession> Create(IEnumerable<RunningSessionDefinition> definitions)
    {
        return definitions.Select(Create).ToList();
    }

    private static RunningSession CreateInterval(IntervalSessionDefinition definition)
    {
        var recovery = definition.Recovery ?? new List<int>();

        var paceKeys = definition.PaceKeys ?? new List<string>();
        if (paceKeys.Count == 1 && definition.Distances.Count > 1)
            paceKeys = Enumerable.Repeat(paceKeys[0], definition.Distances.Count).ToList();

        return new IntervallSession(
            definition.Id,
            definition.Name,
            definition.Date,
            definition.Distances,
            recovery,
            paceKeys,
            definition.Sets,
            definition.SetRecovery,
            definition.WarmupDistance,
            definition.CooldownDistance);
    }

    private static RunningSession CreatePlanned(PlannedSessionDefinition definition)
    {
        return new PlannedRunSession(
            definition.Id,
            definition.Name,
            definition.Date,
            definition.Sequence
                .Select(s => new RunningSegment(s.Type, s.Distance, s.PaceKey, s.Duration))
                .ToList());
    }
}
