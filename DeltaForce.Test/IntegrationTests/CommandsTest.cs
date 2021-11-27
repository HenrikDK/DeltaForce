using DeltaForce.Infrastructure;

namespace DeltaForce.Test.IntegrationTests;

public class CommandTests
{
    private Container _container;
    private IConfiguration _configuration;

    [SetUp]
    public void Setup()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var registry = new WorkerRegistry();
        registry.AddSingleton(_configuration);
        _container = new Container(registry);
    }
/*
    [Test]
    public void Should_insert_merged_schema()
    {
        var insert = _container.GetInstance<IInsertMergedSchema>();

        insert.Execute("some_json", 1);
    }

    [Test]
    public void Should_update_json()
    {
        var update = _container.GetInstance<IUpdateServiceJson>();

        var service = new ServiceDefinition
        {
            Id = 1,
            Status = ServiceStatus.Fetched,
            JsonData = "some_test_json",
        };
            
        update.Execute(new List<ServiceDefinition>{ service });
    }
    
    [Test]
    public void Should_update_service_status()
    {
        var update = _container.GetInstance<Model.Commands.IUpdateServiceStatus>();

        var service = new ServiceDefinition
        {
            Id = 3,
            Status = ServiceStatus.Deployed,
            JsonData = null,
            Retry = 3,
            ServiceUrls = "test_url"
        };
            
        update.Execute(new List<ServiceDefinition>{ service });
    }*/
}