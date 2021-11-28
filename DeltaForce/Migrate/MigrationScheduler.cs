namespace DeltaForce.Migrate;

public interface IMigrationScheduler
{
    void ExecuteWithDelay(CancellationToken token, TimeSpan delay);
}
    
public class MigrationScheduler : IMigrationScheduler
{
    private readonly IProcessMigrationScripts _processMigrationScripts;
    private TimeSpan _delay = TimeSpan.FromMinutes(5);
    
    public MigrationScheduler(IProcessMigrationScripts processMigrationScripts)
    {
        _processMigrationScripts = processMigrationScripts;
    }
    
    public void ExecuteWithDelay(CancellationToken token, TimeSpan delay)
    {
        _delay = delay;
        var stopWatch = new Stopwatch();
        while (!token.IsCancellationRequested)
        {
            stopWatch.Start();
            
            _processMigrationScripts.CloneRepoAndProcessScripts();

            WaitForNextExecution(token, stopWatch);
        }
    }

    private void WaitForNextExecution(CancellationToken token, Stopwatch stopWatch)
    {
        stopWatch.Stop();
        var elapsed = stopWatch.Elapsed;
        var calculated = _delay.Subtract(elapsed);
        stopWatch.Reset();

        if (elapsed < _delay)
        {
            Task.Delay(calculated, token).Wait(token);
        }
    }
}