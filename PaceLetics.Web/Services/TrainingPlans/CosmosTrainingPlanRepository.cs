using System.Threading.Channels;
using AthleteDataAccessLibrary;
using AthleteDataAccessLibrary.Contracts;
using PaceLetics.TrainingPlanModule.CodeBase.Definitions;
using PaceLetics.TrainingPlanModule.CodeBase.Repositories;

namespace PaceLetics.Web.Services.TrainingPlans;

public sealed class CosmosTrainingPlanRepository : BackgroundService, ITrainingPlanRepository
{
    private readonly IDataAccess _data;
    private readonly AthleteDataOptions _options;
    private readonly ILogger<CosmosTrainingPlanRepository> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly object _syncRoot = new();
    private readonly Dictionary<string, TrainingPlanDefinition> _plans = new(StringComparer.OrdinalIgnoreCase);
    private readonly Channel<TrainingPlanDefinition> _writes = Channel.CreateUnbounded<TrainingPlanDefinition>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    public CosmosTrainingPlanRepository(
        IDataAccess data,
        AthleteDataOptions options,
        JsonTrainingPlanRepository seedRepository,
        ILogger<CosmosTrainingPlanRepository> logger,
        TimeProvider? timeProvider = null)
    {
        _data = data;
        _options = options;
        _logger = logger;
        _timeProvider = timeProvider ?? TimeProvider.System;

        foreach (var plan in seedRepository.Load())
            _plans[plan.Id] = Clone(plan);
    }

    public IReadOnlyList<TrainingPlanDefinition> Load()
    {
        lock (_syncRoot)
            return _plans.Values.Select(Clone).ToList();
    }

    public void Save(TrainingPlanDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        var snapshot = Clone(definition);
        lock (_syncRoot)
            _plans[snapshot.Id] = snapshot;

        if (!_writes.Writer.TryWrite(Clone(snapshot)))
            throw new InvalidOperationException("The training-plan persistence queue is unavailable.");
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await LoadPersistedPlansAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var plan in _writes.Reader.ReadAllAsync(stoppingToken))
                await PersistWithRetryAsync(plan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The persistent training-plan store stopped unexpectedly.");
        }
        finally
        {
            while (_writes.Reader.TryRead(out var pending))
            {
                try
                {
                    await PersistAsync(pending, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "A pending training plan could not be persisted during shutdown.");
                }
            }
        }
    }

    private async Task LoadPersistedPlansAsync(CancellationToken cancellationToken)
    {
        var documents = await _data.LoadPartitionData<TrainingPlanStoreDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            TrainingPlanStoreDocument.PartitionKeyValue,
            TrainingPlanStoreDocument.DocumentTypeValue,
            cancellationToken);

        var persistedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        lock (_syncRoot)
        {
            foreach (var document in documents)
            {
                var plan = JsonTrainingPlanRepository.Deserialize(document.Json, document.Id);
                _plans[plan.Id] = plan;
                persistedIds.Add(plan.Id);
            }
        }

        foreach (var seed in Load().Where(plan => !persistedIds.Contains(plan.Id)))
            await PersistAsync(seed, cancellationToken);
    }

    private async Task PersistWithRetryAsync(TrainingPlanDefinition plan, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await PersistAsync(plan, cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < 3 && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Persisting training plan {PlanId} failed on attempt {Attempt}.", plan.Id, attempt);
                await Task.Delay(TimeSpan.FromSeconds(attempt), _timeProvider, cancellationToken);
            }
        }
    }

    private Task PersistAsync(TrainingPlanDefinition plan, CancellationToken cancellationToken)
    {
        var document = new TrainingPlanStoreDocument
        {
            Id = plan.Id,
            Json = JsonTrainingPlanRepository.Serialize(plan),
            UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };
        return _data.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            document,
            TrainingPlanStoreDocument.PartitionKeyValue,
            cancellationToken);
    }

    private static TrainingPlanDefinition Clone(TrainingPlanDefinition source)
    {
        return JsonTrainingPlanRepository.Deserialize(
            JsonTrainingPlanRepository.Serialize(source),
            source.Id);
    }
}
