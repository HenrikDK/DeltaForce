namespace DeltaForce.Model.Dialects;

public interface ISqlDialect
{
    string CreateDb { get; }
    
    string GetScript { get; }
    string InsertScript { get; }
    string UpdateScript { get; }
    
    string GetState { get; }
    string SaveState { get; }
}
