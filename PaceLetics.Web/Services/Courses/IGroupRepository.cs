namespace PaceLetics.Web.Services.Courses;

public interface IGroupRepository
{
    Task<IReadOnlyList<GroupDocument>> GetGroupsAsync();

    Task<GroupDocument?> GetGroupAsync(string groupId);

    Task UpsertGroupAsync(GroupDocument group);

    Task DeleteGroupAsync(string groupId);

    Task<IReadOnlyList<GroupMembershipDocument>> GetMembershipsForAthleteAsync(string athleteUserId);

    Task<IReadOnlyList<GroupMembershipDocument>> GetMembershipsForGroupAsync(string groupId);

    Task<GroupMembershipDocument?> GetMembershipAsync(string groupId, string athleteUserId);

    Task UpsertMembershipAsync(GroupMembershipDocument membership);

    Task<IReadOnlyList<TrainingPlanPublicationDocument>> GetTrainingPlanPublicationsAsync();

    Task UpsertTrainingPlanPublicationAsync(TrainingPlanPublicationDocument publication);

    Task DeleteTrainingPlanPublicationAsync(string publicationId);
}
