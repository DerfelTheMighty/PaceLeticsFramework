namespace PaceLetics.Web.Services.Courses;

public interface ICourseRunningAnalysisRegistrationAdapter
{
    Task OnRegisteredAsync(
        CourseDocument course,
        CourseEventDocument courseEvent,
        CourseEventRegistrationDocument registration,
        CancellationToken cancellationToken = default);
}
