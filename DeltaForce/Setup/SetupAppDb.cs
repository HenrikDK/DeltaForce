using DeltaForce.Model.Repositories;

namespace DeltaForce.Setup;

public interface ISetupAppDb
{
    void EnsureAppDbIsReady();
}
    
public class SetupAppDb : ISetupAppDb
{
    private readonly IScriptRepository _scriptRepository;
    private readonly ILogger<SetupAppDb> _logger;

    public SetupAppDb(IScriptRepository scriptRepository,
        ILogger<SetupAppDb> logger)
    {
        _scriptRepository = scriptRepository;
        _logger = logger;
    }
       
    public void EnsureAppDbIsReady()
    {
        _logger.LogInformation("Checking schema exists");

        var appSchemaExists = _scriptRepository.Check();
        if (appSchemaExists)
        {
            _logger.LogInformation("App schema exists skipping setup");
            return;
        }
        
        _logger.LogInformation("Schema not found, creating..");
        
        _scriptRepository.Create();
        
        _logger.LogInformation("App schema created.");
    }
}