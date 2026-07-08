using Microsoft.Extensions.Localization;
using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.Web.Services.Courses;

public sealed class GroupService : IGroupService
{
    private readonly IGroupRepository _repository;
    private readonly IStringLocalizer<GroupService>? _localizer;

    public GroupService(IGroupRepository repository, IStringLocalizer<GroupService>? localizer = null)
    {
        _repository = repository;
        _localizer = localizer;
    }

    public async Task<IReadOnlyList<GroupOverview>> GetGroupsForAthleteAsync(string athleteUserId)
    {
        var groups = (await _repository.GetGroupsAsync())
            .Where(group => group.IsPublished)
            .ToList();
        var memberships = await _repository.GetMembershipsForAthleteAsync(athleteUserId);
        var membershipByGroup = memberships
            .GroupBy(membership => membership.GroupId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(membership => membership.RequestedAt).First(), StringComparer.OrdinalIgnoreCase);

        return groups
            .Select(group =>
            {
                membershipByGroup.TryGetValue(group.Id, out var membership);
                return new GroupOverview(group, membership);
            })
            .OrderByDescending(group => group.IsJoined)
            .ThenByDescending(group => group.IsPending)
            .ThenBy(group => group.Group.Name)
            .ToList();
    }

    public async Task<IReadOnlyList<GroupDocument>> GetGroupsForTrainerAsync(string trainerUserId)
    {
        if (string.IsNullOrWhiteSpace(trainerUserId))
            return Array.Empty<GroupDocument>();

        return (await _repository.GetGroupsAsync())
            .Where(group => string.Equals(group.CreatedByTrainerUserId, trainerUserId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(group => group.Name)
            .ToList();
    }

    public async Task<IReadOnlyList<GroupMembershipDocument>> GetMembershipsForGroupAsync(string groupId, string requestingTrainerUserId)
    {
        var group = await RequireGroupTrainerAsync(groupId, requestingTrainerUserId);
        return (await _repository.GetMembershipsForGroupAsync(group.Id))
            .OrderBy(membership => membership.Status)
            .ThenBy(membership => membership.RequestedAt)
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetActiveGroupIdsForAthleteAsync(string athleteUserId)
    {
        return (await _repository.GetMembershipsForAthleteAsync(athleteUserId))
            .Where(membership => string.Equals(membership.Status, GroupMembershipStatus.Active, StringComparison.OrdinalIgnoreCase))
            .Select(membership => membership.GroupId)
            .Where(groupId => !string.IsNullOrWhiteSpace(groupId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public Task<GroupDocument?> GetGroupAsync(string groupId)
    {
        return _repository.GetGroupAsync(groupId);
    }

    public async Task<GroupDocument> CreateGroupAsync(GroupCreateRequest request, string trainerUserId, string trainerDisplayName)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(trainerUserId))
            throw new InvalidOperationException(Text("GroupTrainerRequired", "A group needs a signed-in coach."));

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException(Text("GroupNameRequired", "A group needs a name."));

        var groupId = CreateGroupId(request.Name);
        var group = new GroupDocument
        {
            Id = groupId,
            GroupId = groupId,
            Slug = groupId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            CreatedByTrainerUserId = trainerUserId,
            CreatedByDisplayName = NormalizeDisplayName(trainerDisplayName, trainerUserId),
            CreatedAt = DateTime.UtcNow,
            IsPublished = request.IsPublished,
            JoinMode = NormalizeJoinMode(request.JoinMode)
        };

        await _repository.UpsertGroupAsync(group);
        return group;
    }

    public async Task<GroupDocument> UpdateGroupAsync(string groupId, GroupCreateRequest request, string requestingTrainerUserId)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException(Text("GroupNameRequired", "A group needs a name."));

        var group = await RequireGroupTrainerAsync(groupId, requestingTrainerUserId);
        group.Name = request.Name.Trim();
        group.Description = request.Description?.Trim() ?? string.Empty;
        group.IsPublished = request.IsPublished;
        group.JoinMode = NormalizeJoinMode(request.JoinMode);

        await _repository.UpsertGroupAsync(group);
        return group;
    }

    public async Task DeleteGroupAsync(string groupId, string requestingTrainerUserId)
    {
        var group = await RequireGroupTrainerAsync(groupId, requestingTrainerUserId);
        await _repository.DeleteGroupAsync(group.Id);
    }

    public async Task<GroupMembershipDocument> JoinGroupAsync(string groupId, string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException(Text("JoinGroupAthleteRequired", "Joining a group requires a signed-in athlete."));

        var group = await _repository.GetGroupAsync(groupId)
            ?? throw new InvalidOperationException(Text("GroupNotFound", "The group was not found."));

        if (!group.IsPublished)
            throw new InvalidOperationException(Text("GroupNotAvailable", "This group is not available."));

        var now = DateTime.UtcNow;
        var membership = await _repository.GetMembershipAsync(group.Id, athleteUserId)
            ?? new GroupMembershipDocument
            {
                GroupId = group.Id,
                AthleteUserId = athleteUserId,
                RequestedAt = now
            };

        membership.Status = string.Equals(group.JoinMode, GroupJoinModes.ApprovalRequired, StringComparison.OrdinalIgnoreCase)
            ? GroupMembershipStatus.Pending
            : GroupMembershipStatus.Active;
        membership.CancelledAt = null;
        membership.RejectedAt = null;
        membership.RejectedByTrainerUserId = string.Empty;

        if (membership.Status == GroupMembershipStatus.Active)
        {
            membership.ApprovedAt = now;
            membership.ApprovedByTrainerUserId = group.CreatedByTrainerUserId;
        }
        else
        {
            membership.ApprovedAt = null;
            membership.ApprovedByTrainerUserId = string.Empty;
        }

        if (membership.RequestedAt == default)
            membership.RequestedAt = now;

        await _repository.UpsertMembershipAsync(membership);
        return membership;
    }

    public async Task<GroupMembershipDocument> LeaveGroupAsync(string groupId, string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException(Text("LeaveGroupAthleteRequired", "Leaving a group requires a signed-in athlete."));

        var membership = await _repository.GetMembershipAsync(groupId, athleteUserId)
            ?? throw new InvalidOperationException(Text("GroupMembershipNotFound", "You are not a member of this group."));

        membership.Status = GroupMembershipStatus.Cancelled;
        membership.CancelledAt = DateTime.UtcNow;

        await _repository.UpsertMembershipAsync(membership);
        return membership;
    }

    public async Task<GroupMembershipDocument> ApproveMembershipAsync(string groupId, string athleteUserId, string requestingTrainerUserId)
    {
        var group = await RequireGroupTrainerAsync(groupId, requestingTrainerUserId);
        var membership = await _repository.GetMembershipAsync(group.Id, athleteUserId)
            ?? throw new InvalidOperationException(Text("GroupMembershipNotFound", "The membership request was not found."));

        membership.Status = GroupMembershipStatus.Active;
        membership.ApprovedAt = DateTime.UtcNow;
        membership.ApprovedByTrainerUserId = requestingTrainerUserId;
        membership.CancelledAt = null;
        membership.RejectedAt = null;
        membership.RejectedByTrainerUserId = string.Empty;

        await _repository.UpsertMembershipAsync(membership);
        return membership;
    }

    public async Task<GroupMembershipDocument> RejectMembershipAsync(string groupId, string athleteUserId, string requestingTrainerUserId)
    {
        var group = await RequireGroupTrainerAsync(groupId, requestingTrainerUserId);
        var membership = await _repository.GetMembershipAsync(group.Id, athleteUserId)
            ?? throw new InvalidOperationException(Text("GroupMembershipNotFound", "The membership request was not found."));

        membership.Status = GroupMembershipStatus.Rejected;
        membership.RejectedAt = DateTime.UtcNow;
        membership.RejectedByTrainerUserId = requestingTrainerUserId;
        membership.ApprovedAt = null;
        membership.ApprovedByTrainerUserId = string.Empty;

        await _repository.UpsertMembershipAsync(membership);
        return membership;
    }

    public Task<IReadOnlyList<TrainingPlanPublicationDocument>> GetTrainingPlanPublicationsAsync()
    {
        return _repository.GetTrainingPlanPublicationsAsync();
    }

    public async Task UpsertTrainingPlanPublicationAsync(
        string trainingPlanId,
        FeedTarget target,
        string publishedByUserId,
        DateTime? visibleFrom = null,
        DateTime? visibleUntil = null)
    {
        if (string.IsNullOrWhiteSpace(trainingPlanId))
            throw new InvalidOperationException(Text("TrainingPlanRequired", "Please select a training plan."));

        if (string.IsNullOrWhiteSpace(publishedByUserId))
            throw new InvalidOperationException(Text("PublicationTrainerRequired", "Publishing a plan requires a signed-in coach."));

        var normalizedTarget = (target is null || target.IsEmpty)
            ? FeedTarget.Global()
            : target.NormalizeCopy();
        normalizedTarget.Validate();

        if (string.Equals(normalizedTarget.TargetType, FeedTargetTypes.Group, StringComparison.OrdinalIgnoreCase))
            _ = await RequireGroupTrainerAsync(normalizedTarget.TargetId, publishedByUserId);

        var publication = new TrainingPlanPublicationDocument
        {
            TrainingPlanId = trainingPlanId.Trim(),
            Target = normalizedTarget,
            PublishedAt = DateTime.UtcNow,
            PublishedByUserId = publishedByUserId,
            VisibleFrom = visibleFrom,
            VisibleUntil = visibleUntil
        };
        publication.Id = CourseDocumentIds.TrainingPlanPublication(publication.TrainingPlanId, publication.Target);

        publication.ToContentPublication().Validate();
        await _repository.UpsertTrainingPlanPublicationAsync(publication);
    }

    public async Task RemoveTrainingPlanPublicationAsync(string publicationId, string requestingTrainerUserId)
    {
        if (string.IsNullOrWhiteSpace(publicationId))
            return;

        var publication = (await _repository.GetTrainingPlanPublicationsAsync())
            .FirstOrDefault(publication => string.Equals(publication.Id, publicationId, StringComparison.OrdinalIgnoreCase));

        if (publication is null)
            return;

        if (string.Equals(publication.Target.TargetType, FeedTargetTypes.Group, StringComparison.OrdinalIgnoreCase))
            _ = await RequireGroupTrainerAsync(publication.Target.TargetId, requestingTrainerUserId);

        await _repository.DeleteTrainingPlanPublicationAsync(publication.Id);
    }

    public async Task<IReadOnlyList<string>> GetVisibleTrainingPlanIdsForAthleteAsync(
        string athleteUserId,
        IReadOnlyList<CourseDocument> joinedCourses)
    {
        var now = DateTime.UtcNow;
        var activeGroupIds = (await GetActiveGroupIdsForAthleteAsync(athleteUserId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var joinedCourseIds = joinedCourses
            .Select(course => course.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var publication in await _repository.GetTrainingPlanPublicationsAsync())
        {
            if (!IsVisibleToAthlete(publication.ToContentPublication(), joinedCourseIds, activeGroupIds, now))
                continue;

            ids.Add(publication.TrainingPlanId);
        }

        foreach (var course in joinedCourses)
        {
            foreach (var publication in course.TrainingPlanPublications)
            {
                if (publication.IsVisibleInCourse(course.Id, now))
                    ids.Add(publication.TrainingPlanId);
            }
        }

        return ids
            .Where(planId => !string.IsNullOrWhiteSpace(planId))
            .ToList();
    }

    private async Task<GroupDocument> RequireGroupTrainerAsync(string groupId, string trainerUserId)
    {
        var group = await _repository.GetGroupAsync(groupId)
            ?? throw new InvalidOperationException(Text("GroupNotFound", "The group was not found."));

        if (!string.Equals(group.CreatedByTrainerUserId, trainerUserId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(Text("GroupTrainerOnly", "Only the coach who created this group can manage it."));

        return group;
    }

    private static bool IsVisibleToAthlete(
        ContentPublication publication,
        IReadOnlySet<string> joinedCourseIds,
        IReadOnlySet<string> activeGroupIds,
        DateTime now)
    {
        if (!publication.IsVisibleAt(now))
            return false;

        var target = publication.Target.NormalizeCopy();
        if (target.IsGlobal)
            return true;

        if (string.Equals(target.TargetType, FeedTargetTypes.Course, StringComparison.OrdinalIgnoreCase))
            return joinedCourseIds.Contains(target.TargetId);

        if (string.Equals(target.TargetType, FeedTargetTypes.Group, StringComparison.OrdinalIgnoreCase))
            return activeGroupIds.Contains(target.TargetId);

        return false;
    }

    private static string NormalizeJoinMode(string? joinMode)
    {
        return string.Equals(joinMode, GroupJoinModes.ApprovalRequired, StringComparison.OrdinalIgnoreCase)
            ? GroupJoinModes.ApprovalRequired
            : GroupJoinModes.Open;
    }

    private static string CreateGroupId(string name)
    {
        var slug = new string(name
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray());
        slug = string.Join("-", slug.Split('-', StringSplitOptions.RemoveEmptyEntries));

        if (string.IsNullOrWhiteSpace(slug))
            slug = "gruppe";

        var suffix = Guid.NewGuid().ToString("N")[..8];
        return $"{slug}-{suffix}";
    }

    private static string NormalizeDisplayName(string displayName, string fallbackUserId)
    {
        return string.IsNullOrWhiteSpace(displayName)
            ? fallbackUserId
            : displayName.Trim();
    }

    private string Text(string key, string fallback)
    {
        if (_localizer is null)
            return fallback;

        var localized = _localizer[key];
        return localized.ResourceNotFound ? fallback : localized.Value;
    }
}
