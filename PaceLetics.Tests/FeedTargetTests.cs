using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.Tests;

public sealed class FeedTargetTests
{
    [Fact]
    public void CourseTarget_NormalizesAndMatchesIgnoringCase()
    {
        var target = FeedTarget.Course(" Course-1 ");
        var normalized = target.NormalizeCopy();

        Assert.Equal(FeedTargetTypes.Course, normalized.TargetType);
        Assert.Equal("Course-1", normalized.TargetId);
        Assert.True(normalized.Matches(new FeedTarget
        {
            TargetType = "COURSE",
            TargetId = "course-1"
        }));
    }

    [Fact]
    public void GlobalTarget_DropsTargetIdWhenNormalized()
    {
        var normalized = new FeedTarget
        {
            TargetType = "GLOBAL",
            TargetId = "unused"
        }.NormalizeCopy();

        Assert.Equal(FeedTargetTypes.Global, normalized.TargetType);
        Assert.Empty(normalized.TargetId);
        Assert.True(normalized.IsGlobal);
    }

    [Fact]
    public void AddressedTarget_RequiresTargetId()
    {
        var target = new FeedTarget
        {
            TargetType = FeedTargetTypes.Course
        };

        Assert.Throws<InvalidOperationException>(() => target.Validate());
    }

    [Fact]
    public void ContentPublication_IsVisibleOnlyForMatchingTargetAndWindow()
    {
        var now = DateTime.UtcNow;
        var publication = new ContentPublication
        {
            ContentType = PublishedContentTypes.TrainingPlan,
            ContentId = "plan-1",
            Target = FeedTarget.Course("course-1"),
            PublishedAt = now.AddDays(-1),
            VisibleFrom = now.AddMinutes(-5),
            VisibleUntil = now.AddMinutes(5)
        };

        Assert.True(publication.IsVisibleFor(FeedTarget.Course("course-1"), now));
        Assert.False(publication.IsVisibleFor(FeedTarget.Course("course-2"), now));
        Assert.False(publication.IsVisibleFor(FeedTarget.Course("course-1"), now.AddMinutes(6)));
    }
}
