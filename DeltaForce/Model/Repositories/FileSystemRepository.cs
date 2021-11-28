namespace DeltaForce.Model.Repositories;

public interface IFileSystemRepository
{
    bool FileExists(string path);
    string GetFileContents(string path);
    void CleanWorkspace(string workspacePath);
    List<string> GetScripts(string workspacePath, string subFolder, string fileExtension = ".sql");
}
    
public class FileSystemRepository : IFileSystemRepository
{
    private readonly ILogger<FileSystemRepository> _logger;

    public FileSystemRepository(ILogger<FileSystemRepository> logger)
    {
        _logger = logger;
    }
    
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public string GetFileContents(string path)
    {
        return File.ReadAllText(path);
    }
    
    public void CleanWorkspace(string workspacePath)
    {
        _logger.LogInformation("Cleaning workspace");
        var folders = Directory.GetDirectories(workspacePath);
        folders.ForEach(x =>
        {
            _logger.LogInformation($"Deleting folder: {x}");
            Directory.Delete(x, true);
        });
    }
    
    public List<string> GetScripts(string workspacePath, string subFolder, string fileExtension = ".sql")
    {
        var localRepo = Path.Combine(workspacePath, "working" , subFolder);
        
        _logger.LogInformation($"Discovering all scripts in folder {localRepo} ..");
            
        var scripts = Directory.GetFiles(localRepo).Where(x => x.Contains(fileExtension)).ToList();

        return scripts;
    }
}