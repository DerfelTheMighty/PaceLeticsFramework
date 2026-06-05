using MudBlazor;
using PaceLetics.CoreModule.Infrastructure.Models;
using PaceLetics.Web.Services.DashboardMessages;

namespace PaceLetics.Tests;

public sealed class DashboardMessageFeedStateTests
{
    [Fact]
    public void AthleteMessage_DefaultsToUnread()
    {
        var message = CreateMessage("message-1");

        Assert.False(message.IsRead);
        Assert.True(message.IsUnread);
    }

    [Fact]
    public void Create_UsesStableDocumentIdAndPartition()
    {
        var state = AthleteMessageFeedStateDocument.Create("athlete-1");

        Assert.Equal("athlete-message-feed:athlete-1", state.Id);
        Assert.Equal("athlete-message-feed:athlete-1", state.CourseId);
        Assert.Equal("athlete-1", state.AthleteUserId);
        Assert.Equal(AthleteMessageFeedDocumentTypes.State, state.DocumentType);
    }

    [Fact]
    public void MarkRead_MarksSelectedMessagesAsRead()
    {
        var state = AthleteMessageFeedStateDocument.Create("athlete-1");
        var readAt = new DateTime(2026, 6, 5, 8, 30, 0, DateTimeKind.Utc);

        state.MarkRead(new[] { "message-1", "message-2" }, readAt);

        Assert.True(state.IsRead("message-1"));
        Assert.True(state.IsRead("message-2"));
        Assert.False(state.IsRead("message-3"));
        Assert.False(state.IsDeleted("message-1"));
    }

    [Fact]
    public void Delete_MarksMessageAsReadAndDeleted()
    {
        var state = AthleteMessageFeedStateDocument.Create("athlete-1");
        var deletedAt = new DateTime(2026, 6, 5, 9, 0, 0, DateTimeKind.Utc);

        state.Delete("message-1", deletedAt);

        Assert.True(state.IsRead("message-1"));
        Assert.True(state.IsDeleted("message-1"));
        Assert.Equal(deletedAt, state.Messages.Single().ReadAt);
        Assert.Equal(deletedAt, state.Messages.Single().DeletedAt);
    }

    private static AthleteMessage CreateMessage(string id)
    {
        return new AthleteMessage(
            id,
            "Test",
            Severity.Info,
            "Title",
            "Body",
            Icons.Material.Filled.Info,
            null,
            null,
            10,
            Array.Empty<object>());
    }
}
