using PaceLetics.CoreModule.Infrastructure.Constants;
using PaceLetics.RunningModule.CodeBase.Models;

namespace PaceLetics.Tests;

public class RunningSessionResolverTests
{
    [Fact]
    public void Resolve_ComputesSegmentAndLapTimeForKnownPace()
    {
        var session = new TestRunningSession([
            new RunningSegment(SegmentType.Intervall, 1_000, PaceKeys.Intervall)
        ]);

        var resolved = RunningSessionResolver.Resolve(session, PaceModelTests.CreatePaceModel());

        var segment = Assert.Single(resolved.Segments);
        Assert.Equal(TimeSpan.FromMinutes(3), segment.Pace);
        Assert.Equal(TimeSpan.FromMinutes(3), segment.SegmentTime);
        Assert.Equal(TimeSpan.FromSeconds(72), segment.LapTime);
    }

    [Fact]
    public void Resolve_DoesNotFallbackToEasyPaceForUnknownPaceKey()
    {
        var session = new TestRunningSession([
            new RunningSegment(SegmentType.Recovery, 0, "unknown", TimeSpan.FromMinutes(2))
        ]);

        var resolved = RunningSessionResolver.Resolve(session, PaceModelTests.CreatePaceModel());

        var segment = Assert.Single(resolved.Segments);
        Assert.Null(segment.Pace);
        Assert.Equal(TimeSpan.FromMinutes(2), segment.SegmentTime);
        Assert.Null(segment.LapTime);
    }

    [Fact]
    public void Resolve_ComputesRecoveryPaceFromEasyPacePlusThirtySeconds()
    {
        var session = new TestRunningSession([
            new RunningSegment(SegmentType.Recovery, 200, PaceKeys.Recovery)
        ]);

        var resolved = RunningSessionResolver.Resolve(session, PaceModelTests.CreatePaceModel());

        var segment = Assert.Single(resolved.Segments);
        Assert.Equal(TimeSpan.FromMinutes(6).Add(TimeSpan.FromSeconds(30)), segment.Pace);
        Assert.Equal(TimeSpan.FromSeconds(78), segment.SegmentTime);
        Assert.Null(segment.LapTime);
    }

    [Fact]
    public void IntervallSession_UsesRecoveryPaceForGeneratedRecoverySegments()
    {
        var session = new IntervallSession(
            "interval",
            "Interval",
            new DateTime(2026, 1, 1),
            [200],
            [],
            [PaceKeys.Repetition],
            sets: 2,
            setRecovery: 200);

        Assert.Equal(PaceKeys.Repetition, session.Sequence[0].PaceKey);
        Assert.Equal(PaceKeys.Recovery, session.Sequence[1].PaceKey);
        Assert.Equal(SegmentType.SetRecovery, session.Sequence[1].Type);
        Assert.Equal(PaceKeys.Repetition, session.Sequence[2].PaceKey);
    }

    [Fact]
    public void Resolve_RejectsEmptySequences()
    {
        var session = new TestRunningSession([]);

        Assert.Throws<ArgumentException>(() =>
            RunningSessionResolver.Resolve(session, PaceModelTests.CreatePaceModel()));
    }

    private sealed class TestRunningSession(IReadOnlyList<RunningSegment> sequence) : RunningSession(
        "test-session",
        "Test Session",
        new DateTime(2026, 1, 1),
        null,
        null)
    {
        public override int TotalDistance => Sequence.Sum(segment => segment.Distance);

        public override IReadOnlyList<RunningSegment> Sequence { get; } = sequence;
    }
}
