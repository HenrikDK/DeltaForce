using DeltaForce.Infrastructure;

namespace DeltaForce;

public class Program
{
    public static bool Debug = false;

    public static void Main(string[] args)
    {
        if (args.Contains("debug") || Debugger.IsAttached || Environment.GetEnvironmentVariable("debug") != null )
        {
            Debug = true;
        }

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLamar(new WorkerRegistry());
                services.AddHostedService<ServiceHost>();
                if (Debug)
                {
                    services.AddLogging(x =>
                    {
                        x.AddDebug();
                        x.AddConsole();
                    });
                }
            })
            .UseLamar()
            .Build();

        host.Run();
    }
}