using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;
using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Workouts.Logic;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Logic;

public class Exercise : TimedWorkoutElement, IExerciseInfo, IWorkoutElement
{
    private readonly Timeslot[] _timeslots;

    public string Name { get; }
    public string Id { get; }
    public string Description { get; }
    public List<string> Execution { get; }
    public string ImageFilename { get; }
    public Level Level { get; }

    public bool SwitchLeftRight { get; }
    public int SwitchTime { get; }

    public Exercise(ExerciseDefinition definition)
        : base(
            WorkoutElements.Exercise,
            definition.SwitchLeftRight
                ? definition.Duration + definition.SwitchTime
                : definition.Duration
          )
    {
        Name = definition.Name;
        Id = definition.Id;
        Description = definition.Description;
        Execution = definition.Execution ?? new List<string>();
        ImageFilename = definition.ImageFile;
        Level = definition.Level;

        SwitchLeftRight = definition.SwitchLeftRight;
        SwitchTime = definition.SwitchTime;

        _timeslots = SwitchLeftRight
            ? new[]
              {
                  new Timeslot(ExerciseState.Switch, SwitchTime),
                  new Timeslot(ExerciseState.Running, definition.Duration)
              }
            : new[]
              {
                  new Timeslot(ExerciseState.Running, definition.Duration)
              };

        ResetToInitial();
    }
}
