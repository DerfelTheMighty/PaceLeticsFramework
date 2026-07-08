using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.Web.Services.Courses;

public interface IGroupService
{
    Task<IReadOnlyList<GroupOverview>> GetGroupsForAthleteAsync(string athleteUserId);

    Task<IReadOnlyList<GroupDocument>> GetGroupsForTrainerAsync(string trainerUserId);

    Task<IReadOnlyList<GroupMembershipDocument>> GetMembershipsForGroupAsync(string groupId, string requestingTrainerUserId);

    Task<IReadOnlyList<string>> GetActiveGroupIdsForAthleteAsync(string athleteUserId);

    Task<GroupDocument?> GetGroupAsync(string groupId);

    Task<GroupDocument> CreateGroupAsync(GroupCreateRequest request, string trainerUserId, string trainerDisplayName);

    Task<GroupDocument> UpdateGroupAsync(string groupId, GroupCreateRequest request, string requestingTrainerUserId);

    Task DeleteGroupAsync(string groupId, string requestingTrainerUserId);

    Task<GroupMembershipDocument> JoinGroupAsync(string groupId, string athleteUserId);

    Task<GroupMembershipDocument> LeaveGroupAsync(string groupId, string athleteUserId);

    Task<GroupMembershipDocument> ApproveMembershipAsync(string groupId, string athleteUserId, string requestingTrainerUserId);

    Task<GroupMembershipDocument> RejectMembershipAsync(string groupId, string athleteUserId, string requestingTrainerUserId);

    Task<IReadOnlyList<TrainingPlanPublicationDocument>> GetTrainingPlanPublicationsAsync();

    Task UpsertTrainingPlanPublicationAsync(string trainingPlanId, FeedTarget target, string publishedByUserId, DateTime? visibleFrom = null, DateTime? visibleUntil = null);

    Task RemoveTrainingPlanPublicationAsync(string publicationId, string requestingTrainerUserId);

    Task<IReadOnlyList<string>> GetVisibleTrainingPlanIdsForAthleteAsync(string athleteUserId, IReadOnlyList<CourseDocument> joinedCourses);
}
