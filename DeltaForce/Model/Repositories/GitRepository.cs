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
        var cmd = $"clone --depth 1 {_gitRepository.Value} -b {_gitBranch.Value} working";
        
        var localPath = Path.Combine(Directory.GetCurrentDirectory(), workspacePath);

        _logger.LogInformation($"Cloning repo: {_gitRepository.Value} to local folder: {localPath}");

        RunCommandWithOutput(cmd, localPath, "GIT_SSH_COMMAND", $"ssh -i {_gitSshKeyPath.Value}");
    }

    public string GetLatestCommit()
    {
        var cmd = $"ls-remote {_gitRepository.Value} refs/heads/{_gitBranch.Value}";
        
        var latest = RunCommandWithOutput(cmd);

        if (!string.IsNullOrEmpty(latest))
        {
            latest = latest.Split('\t').First();
        }

        _logger.LogInformation($"Repository: {_gitRepository.Value} - branch: {_gitBranch.Value} - latest commit: {latest}");

        return latest;
    }

    public List<string> GetScripts(string workspacePath)
    {
        return _fileSystemRepository.GetScripts(workspacePath, _gitPath.Value);
    }
    
    private string RunCommandWithOutput(string command, string workingDirectory = null, string envKey = null, string envValue = null)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = command,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false,
            };

            if (workingDirectory != null)
            {
                startInfo.WorkingDirectory = workingDirectory;
            }

            if (envKey != null)
            {
                startInfo.EnvironmentVariables.Add(envKey, envValue);
            }

            var proc = new Process{ StartInfo = startInfo };
            proc.Start();
            proc.WaitForExit();
            var result = proc.StandardOutput.ReadToEnd();
            result += proc.StandardError.ReadToEnd();
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $@"Error in command: ""{command}""");
        }

        return "";
    }
}