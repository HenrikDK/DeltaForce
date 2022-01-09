namespace DeltaForce.Infrastructure;

public class WorkerRegistry : ServiceRegistry
{
    public WorkerRegistry()
    {
        For<IConnectionFactory>().Use<ConnectionFactory>().Singleton();
    }
}