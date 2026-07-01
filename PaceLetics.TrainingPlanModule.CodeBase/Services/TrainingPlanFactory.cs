using PaceLetics.TrainingModule.CodeBase.Running.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;
using PaceLetics.TrainingPlanModule.CodeBase.Definitions;
using PaceLetics.TrainingPlanModule.CodeBase.Interfaces;
using PaceLetics.TrainingPlanModule.CodeBase.Models;

namespace PaceLetics.TrainingPlanModule.CodeBase.Services;

public sealed class TrainingPlanFactory : ITrainingPlanFactory
{
    private readonly IRunningSessionFactory _runningSessionFactory;
    private readonly ITrainingPlanDefinitionValidator _validator;

    public TrainingPlanFactory(
        IRunningSessionFactory runningSessionFactory,
        IWorkoutCatalog? workoutCatalog = null,
        ITrainingPlanDefinitionValidator? validator = null)
    {
        _runningSessionFactory = runningSessionFactory;
        _validator = validator ?? new TrainingPlanDefinitionValidator(workoutCatalog);
    }

    public TrainingPlan Create(TrainingPlanDefinition definition)
    {
        try
        {
            _validator.Validate(definition);

            return new TrainingPlan(
                definition.Id,
                definition.Name,
                definition.Sessions.Select(CreateTrainingSession),
                definition.Blocks.Select(CreateTrainingPlanBlock));
        }
        catch (TrainingPlanDefinitionValidationException ex)
        {
            throw new InvalidDataException(ex.Message, ex);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidDataException($"Training plan definition '{definition.Id}' is invalid.", ex);
        }
    }

    private static TrainingPlanBlock CreateTrainingPlanBlock(TrainingPlanBlockDefinition definition)
    {
        return new TrainingPlanBlock(
            definition.Id,
            definition.Name,
            definition.SessionIds,
            definition.Order,
            definition.Focus,
            definition.Structure,
            definition.Description);
    }

    public IReadOnlyList<TrainingPlan> Create(IEnumerable<TrainingPlanDefinition> definitions)
    {
        return definitions.Select(Create).ToList();
    }

    private TrainingSession CreateTrainingSession(TrainingSessionDefinition definition)
    {
        var runs = definition.Runs
            .Select(_runningSessionFactory.Create)
            .ToList();

        var workouts = definition.Workouts.ToList();
        var primaryRun = runs.FirstOrDefault();
        var appointment = definition.Appointment ?? TrainingSessionAppointment.Empty;

        var date = definition.Date != default
            ? definition.Date
            : appointment.StartsAt?.Date
                ?? primaryRun?.Date
                ?? throw new InvalidDataException($"Training session '{definition.Id}' must define a date when it has no runs or appointment.");

        var id = !string.IsNullOrWhiteSpace(definition.Id)
            ? definition.Id
            : primaryRun?.Id
                ?? CreateSessionId(name: definition.Name, date)
                ?? throw new InvalidDataException("Training session id must not be empty.");

        var name = !string.IsNullOrWhiteSpace(definition.Name)
            ? definition.Name
            : primaryRun?.Name ?? id;

        return new TrainingSession(
            id,
            name,
            date,
            runs,
            workouts,
            definition.Warmup,
            definition.Drills,
            definition.TrainingEffect,
            appointment);
    }

    private static string? CreateSessionId(string name, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var normalizedName = name
            .Trim()
            .ToLowerInvariant()
            .Replace(' ', '-');

        return $"{date:yyyy-MM-dd}-{normalizedName}";
    }
}
