using AthleteDataAccessLibrary.Contracts;
using Microsoft.AspNetCore.Identity;
using PaceLetics.AthleteModule.CodeBase.Models;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;
using PaceLetics.Web.Data;
using PaceLetics.Web.Services.Courses;

namespace PaceLetics.Web.Services.RunningAnalysis;

public sealed class CourseRunningAnalysisRegistrationAdapter : ICourseRunningAnalysisRegistrationAdapter
{
    private readonly IRunningAnalysisService _runningAnalysisService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAthleteData _athleteData;
    private readonly ILogger<CourseRunningAnalysisRegistrationAdapter> _logger;

    public CourseRunningAnalysisRegistrationAdapter(
        IRunningAnalysisService runningAnalysisService,
        UserManager<ApplicationUser> userManager,
        IAthleteData athleteData,
        ILogger<CourseRunningAnalysisRegistrationAdapter> logger)
    {
        _runningAnalysisService = runningAnalysisService;
        _userManager = userManager;
        _athleteData = athleteData;
        _logger = logger;
    }

    public async Task OnRegisteredAsync(
        CourseDocument course,
        CourseEventDocument courseEvent,
        CourseEventRegistrationDocument registration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(registration.AthleteUserId);
            var athlete = await _athleteData.GetAthlete(registration.AthleteUserId);
            var email = user?.Email ?? string.Empty;

            await _runningAnalysisService.RegisterParticipantAsync(
                new RunningAnalysisRegistration(
                    ExternalEventId: courseEvent.Id,
                    CourseId: course.Id,
                    EventTitle: courseEvent.Title,
                    StartsAt: courseEvent.StartsAt,
                    EndsAt: courseEvent.EndsAt,
                    AthleteUserId: registration.AthleteUserId,
                    DisplayName: GetDisplayName(athlete, user?.UserName ?? registration.AthleteUserId),
                    Email: email,
                    RegistrationId: registration.Id,
                    RegisteredAt: registration.RegisteredAt),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Running analysis provisioning failed for event {EventId} and athlete {AthleteUserId}.",
                courseEvent.Id,
                registration.AthleteUserId);
        }
    }

    private static string GetDisplayName(AthleteModel? athlete, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(athlete?.PublicProfile?.PublicUserName)
            && athlete.PublicProfile.PublicUserName != "NA")
        {
            return athlete.PublicProfile.PublicUserName;
        }

        if (!string.IsNullOrWhiteSpace(athlete?.Name) && athlete.Name != "NA")
            return athlete.Name;

        return fallback;
    }
}
