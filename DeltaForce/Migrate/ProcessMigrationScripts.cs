using DeltaForce.Model;
using DeltaForce.Model.Enums;
using DeltaForce.Model.Repositories;

namespace DeltaForce.Migrate;

public interface IProcessMigrationScripts
{
    void CloneRepoAndProcessScripts();
}
    
public class ProcessMigrationScripts : IProcessMigrationScripts
{
    private readonly IGitRepository _gitRepository;
    private readonly IFileSystemRepository _fileSystemRepository;
    private readonly IScriptRepository _scriptRepository;
    private readonly IStateRepository _stateRepository;
    private readonly ILogger<ProcessMigrationScripts> _logger;
    private readonly IApplyMigrationScript _applyMigrationScript;
    private static readonly Gauge PendingScripts = Metrics.CreateGauge("delta_force_pending_scripts_count", "Number of pending sql scripts.");
    private static readonly Histogram JobExecutionTime = Metrics.CreateHistogram("delta_force_apply_script_time_seconds", "Histogram of script execution durations.");
    private readonly string _workspacePath = "delta_force";

    public ProcessMigrationScripts(IGitRepository gitRepository, 
        IFileSystemRepository fileSystemRepository,
        IScriptRepository scriptRepository,
        IStateRepository stateRepository,
        IApplyMigrationScript applyMigrationScript,
        ILogger<ProcessMigrationScripts> logger)
    {
        _gitRepository = gitRepository;
        _fileSystemRepository = fileSystemRepository;
        _scriptRepository = scriptRepository;
        _stateRepository = stateRepository;
        _applyMigrationScript = applyMigrationScript;
        _logger = logger;
    }
       
    public void CloneRepoAndProcessScripts()
    {
        try
        {
            var previous = _stateRepository.Get();
            var current = _gitRepository.GetLatestCommit();
            if (current == previous)
            {
                _logger.LogInformation($"Latest commit: {previous} - repository unchanged, skipping run.");
                return;
            }
            
            CloneRepositoryAndProcessScripts();
            
            _logger.LogInformation($"Updating state to new commit: {current}");
            
            _stateRepository.Update(current);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing repository");
        }
    }

    private void CloneRepositoryAndProcessScripts()
    {
        _fileSystemRepository.CleanWorkspace(_workspacePath);
        _gitRepository.CloneRepository(_workspacePath);
        var scripts = _gitRepository.GetScripts(_workspacePath);
        
        _logger.LogInformation($"Found {scripts.Count} scripts in repository folder with .sql file extension");
        
        var existingScripts = _scriptRepository.GetScripts();
        var pending = scripts
            .Where(x => existingScripts[x].Any() == false ||
                        existingScripts[x].First().Status == ScriptStatus.Failed)
            .OrderBy(x => x)
            .ToList();
        
        PendingScripts.Set(pending.Count);
        if (scripts.Count == 0)
        {
            _logger.LogInformation("No pending scripts found, all done!");
            return;
        }
        
        _logger.LogInformation($"Found {pending.Count} scripts in pending or failed state. Applying in alphabetical sequence..");
        
        pending.ForEach(x => ProcessPendingJob(x, existingScripts[x].FirstOrDefault()));
    }

    private void ProcessPendingJob(string path, Script existingScript)
    {
        using var timer = JobExecutionTime.NewTimer();
        try
        {
            _logger.LogInformation($"Applying script {path}");
            _applyMigrationScript.Apply(path, existingScript);
            PendingScripts.Dec();
        }
        catch (Exception e)
        {
            e.Data.Add("ScriptPath", path);
            e.Data.Add("ExistingScript", existingScript != null);
            
            // TODO, Notify user on script failure?
            _logger.LogError(e, $"Error applying script");
        }
    }
}