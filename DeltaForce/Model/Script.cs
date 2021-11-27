using DeltaForce.Model.Enums;

namespace DeltaForce.Model;

public class Script
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string RepositoryPath { get; set; }
    public string SqlText { get; set; }
    public string Hash { get; set; }
    public ScriptStatus Status { get; set; }
    public string ErrorMessage { get; set; }
}