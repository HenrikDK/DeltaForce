namespace DeltaForce.Model.Repositories;

public interface IGitRepository
{
    void CloneRepository(string workspacePath);
    string GetLatestCommit();
    List<string> GetScripts(string workspacePath);
}
    
public class GitRepository : IGitRepository
{
    private readonly IConfiguration _configuration;
    private readonly IFileSystemRepository _fileSystemRepository;
    private readonly ILogger<GitRepository> _logger;
    private Lazy<string> _gitRepository;
    private Lazy<string> _gitSshKeyPath;
    private Lazy<string> _gitBranch;
    private Lazy<string> _gitPath;

    public GitRepository(IConfiguration configuration,
        IFileSystemRepository fileSystemRepository,
        ILogger<GitRepository> logger)
    {
        _configuration = configuration;
        _fileSystemRepository = fileSystemRepository;
        _logger = logger;
        _gitRepository = new Lazy<string>(() => _configuration.GetValue<string>("git-repository"));
        _gitBranch = new Lazy<string>(() => _configuration.GetValue<string>("git-branch"));
        _gitSshKeyPath = new Lazy<string>(() => _configuration.GetValue<string>("git-ssh-key-path"));
        _gitPath = new Lazy<string>(() => _configuration.GetValue<string>("git-path"));
    }
       
    public void CloneRepository(string workspacePath)
    {
        var cmd = $@"GIT_SSH_COMMAND=""ssh -i {_gitSshKeyPath.Value}"" git clone --depth 1 {_gitRepository.Value} -b {_gitBranch.Value} .";
        
        var localPath = Path.Combine(workspacePath, "working");

        _logger.LogInformation($"Cloning repo: {_gitRepository.Value} to local folder: {localPath}");

        RunCommandWithOutput(cmd, localPath);
    }

    public string GetLatestCommit()
    {
        var cmd = $"git ls-remote {_gitRepository.Value} refs/heads/{_gitBranch.Value}";
        
        var latest = RunCommandWithOutput(cmd);

        _logger.LogInformation($"Repository: {_gitRepository.Value} - branch: {_gitBranch.Value} - latest commit: {latest}");

        return latest;
    }

    public List<string> GetScripts(string workspacePath)
    {
        return _fileSystemRepository.GetScripts(workspacePath, _gitPath.Value);
    }
    
    private string RunCommandWithOutput(string command, string workingDirectory = null)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = command,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            if (null != workingDirectory)
            {
                startInfo.WorkingDirectory = workingDirectory;
            }

            var proc = new Process{ StartInfo = startInfo };
            proc.Start();
            proc.WaitForExit();
            return proc.StandardOutput.ReadToEnd();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $@"Error in command: ""{command}""");
        }

        return "";
    }
}