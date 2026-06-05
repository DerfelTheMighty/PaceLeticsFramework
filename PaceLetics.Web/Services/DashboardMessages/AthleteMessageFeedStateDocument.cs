using PaceLetics.CoreModule.Infrastructure.Interfaces;

namespace PaceLetics.Web.Services.DashboardMessages;

public static class AthleteMessageFeedDocumentTypes
{
    public const string State = "athleteMessageFeedState";
}

public sealed class AthleteMessageFeedStateDocument : IQueryItem
{
    public string Id { get; set; } = string.Empty;

    public string CourseId { get; set; } = string.Empty;

    public string DocumentType { get; set; } = AthleteMessageFeedDocumentTypes.State;

    public string AthleteUserId { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<AthleteMessageStateEntry> Messages { get; set; } = new();

    public static AthleteMessageFeedStateDocument Create(string athleteUserId)
    {
        return new AthleteMessageFeedStateDocument
        {
            Id = AthleteMessageFeedStateIds.Document(athleteUserId),
            CourseId = AthleteMessageFeedStateIds.Partition(athleteUserId),
            AthleteUserId = athleteUserId
        };
    }

    public bool IsRead(string messageId)
    {
        return Find(messageId)?.IsRead == true;
    }

    public bool IsDeleted(string messageId)
    {
        return Find(messageId)?.IsDeleted == true;
    }

    public void MarkRead(IEnumerable<string> messageIds, DateTime readAt)
    {
        foreach (var messageId in messageIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal))
        {
            var entry = GetOrCreate(messageId);
            if (entry.IsRead)
                continue;

            entry.IsRead = true;
            entry.ReadAt = readAt;
        }

        UpdatedAt = readAt;
    }

    public void Delete(string messageId, DateTime deletedAt)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            return;

        var entry = GetOrCreate(messageId);
        entry.IsRead = true;
        entry.ReadAt ??= deletedAt;
        entry.IsDeleted = true;
        entry.DeletedAt = deletedAt;
        UpdatedAt = deletedAt;
    }

    public void Normalize()
    {
        if (string.IsNullOrWhiteSpace(AthleteUserId))
            return;

        Id = AthleteMessageFeedStateIds.Document(AthleteUserId);
        CourseId = AthleteMessageFeedStateIds.Partition(AthleteUserId);
        DocumentType = AthleteMessageFeedDocumentTypes.State;

        Messages = Messages
            .Where(message => !string.IsNullOrWhiteSpace(message.MessageId))
            .GroupBy(message => message.MessageId, StringComparer.Ordinal)
            .Select(group => group.Last())
            .ToList();
    }

    private AthleteMessageStateEntry? Find(string messageId)
    {
        return Messages.FirstOrDefault(message => message.MessageId == messageId);
    }

    private AthleteMessageStateEntry GetOrCreate(string messageId)
    {
        var entry = Find(messageId);
        if (entry is not null)
            return entry;

        entry = new AthleteMessageStateEntry { MessageId = messageId };
        Messages.Add(entry);
        return entry;
    }
}

public sealed class AthleteMessageStateEntry
{
    public string MessageId { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}

public static class AthleteMessageFeedStateIds
{
    public static string Document(string athleteUserId)
    {
        return $"athlete-message-feed:{athleteUserId}";
    }

    public static string Partition(string athleteUserId)
    {
        return $"athlete-message-feed:{athleteUserId}";
    }
}
