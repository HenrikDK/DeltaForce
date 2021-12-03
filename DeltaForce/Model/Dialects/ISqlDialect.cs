namespace DeltaForce.Model.Dialects;

public interface ISqlDialect
{
    string CheckSchema { get; }
    string CreateSchema { get; }
    
    string GetScript { get; }
    string InsertScript { get; }
    string UpdateScript { get; }
    
    string GetState { get; }
    string SaveState { get; }
}
