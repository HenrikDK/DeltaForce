using DeltaForce.Model;
using DeltaForce.Model.Enums;
using DeltaForce.Model.Repositories;

namespace DeltaForce.Migrate;

public interface IApplyMigrationScript
{
    void Apply(string path, Script script);
}
    
public class ApplyMigrationScript : IApplyMigrationScript
{
    private readonly IScriptRepository _scriptRepository;
    private readonly IFileSystemRepository _fileSystemRepository;
    private readonly ILogger<ApplyMigrationScript> _log;

    public ApplyMigrationScript(IScriptRepository scriptRepository,
        IFileSystemRepository fileSystemRepository,
        ILogger<ApplyMigrationScript> log)
    {
        _scriptRepository = scriptRepository;
        _fileSystemRepository = fileSystemRepository;
        _log = log;
    }
       
    public void Apply(string path, Script script)
    {
        if (!_fileSystemRepository.FileExists(path))
        {
            return;
        }
        
        var sql = _fileSystemRepository.GetFileContents(path);
        var newHash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(sql)).ToString();
        if (script != null && script.Status == ScriptStatus.Failed)
        {
            if (script.Hash == newHash)
            {
                _log.LogDebug($"Skipping unchanged & failed script {path}");
                return;
            }

            _log.LogInformation($"Re-processing failed script {path}");
            script.SqlText = sql;
            script.Hash = newHash;
        }
            
        if (script == null)
        {
            _log.LogInformation($"Processing new script {path}");

            script = new Script
            {
                Status = ScriptStatus.Waiting,
                Hash = newHash,
                RepositoryPath = path,
                SqlText = sql,
                FileName = new FileInfo(path).Name
            };
        }
            
        ApplyScript(script);
            
        if (script.Id == 0)
        {
            _scriptRepository.Insert(script);
        }
        else
        {
            _scriptRepository.Update(script);
        }
    }

    private void ApplyScript(Script script)
    {
        try
        {
            using (var scope = new TransactionScope())
            {
                _scriptRepository.Apply(script.SqlText);
                scope.Complete();
            }
                
            _log.LogInformation($"Script {script.FileName} applied.");
                
            script.Status = ScriptStatus.Processed;
        }
        catch (Exception e)
        {
            script.ErrorMessage = e.Message;
            script.Status = ScriptStatus.Failed;
            _log.LogError(e, "Error applying script");
        }
    }
}