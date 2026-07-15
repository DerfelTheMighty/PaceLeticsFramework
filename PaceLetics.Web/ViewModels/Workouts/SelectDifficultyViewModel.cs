using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;
using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;

namespace PaceLetics.Web.ViewModels.Workouts;

public sealed class SelectDifficultyViewModel
{
    private const string DefaultPreviewImage = "images/exercises/no_image.png";
    private readonly IWorkoutCatalog _workoutCatalog;
    private readonly IWorkoutService _workoutService;
    private readonly List<string> _variants = new();

    public SelectDifficultyViewModel(IWorkoutCatalog workoutCatalog, IWorkoutService workoutService)
    {
        _workoutCatalog = workoutCatalog;
        _workoutService = workoutService;
    }

    public IReadOnlyList<string> Variants => _variants;
    public string PreviewImage { get; private set; } = DefaultPreviewImage;
    public string PreviewDescription { get; private set; } = string.Empty;
    public int PreviewExerciseCount { get; private set; }
    public int Sets { get; set; } = 1;
    public int Rounds { get; set; } = 1;

    public void Initialize(string workoutName)
    {
        _variants.Clear();
        _variants.AddRange(_workoutCatalog.GetWorkoutIdsByName(workoutName));

        PreviewImage = DefaultPreviewImage;
        PreviewDescription = string.Empty;
        PreviewExerciseCount = 0;

        if (_variants.Count == 0)
            return;

        var preview = _workoutCatalog.GetWorkoutPreview(_variants.First());
        PreviewDescription = preview.Description;
        PreviewExerciseCount = preview.Count;

        var imageFile = preview.Exercises.FirstOrDefault()?.Imagefile;
        if (!string.IsNullOrWhiteSpace(imageFile))
            PreviewImage = $"images/exercises/{imageFile}";
    }

    public Level GetLevel(string workoutId)
    {
        return _workoutCatalog.GetDefinition(workoutId).Level;
    }

    public void StartWorkout(string workoutId)
    {
        _workoutService.SetActiveWorkout(workoutId, Sets, Rounds);
    }
}
