using DeltaForce.Infrastructure;
using DeltaForce.Model;
using DeltaForce.Model.Enums;
using DeltaForce.Model.Repositories;

namespace DeltaForce.Test.IntegrationTests;

public class MySqlDialectTest
{
    private Container _container;
    private IConfiguration _configuration;

    [SetUp]
    public void Setup()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.mysql.json", optional: false, reloadOnChange: true)
            .Build();

        var registry = new WorkerRegistry();
        registry.AddSingleton(_configuration);
        _container = new Container(registry);
    }

    [Test]
    public void Should_check_if_schema_exists()
    {
        var scriptRepository = _container.GetInstance<IScriptRepository>();

        var check = scriptRepository.Check();
        if (check)
        {
            scriptRepository.Apply("drop schema DeltaForce;");
        }
        
        check = scriptRepository.Check();
        check.Should().BeFalse();
    }

    [Test]
    public void Should_create_schema()
    {
        var scriptRepository = _container.GetInstance<IScriptRepository>();
        var check = scriptRepository.Check();
        if (check)
        {
            scriptRepository.Apply("drop schema DeltaForce;");
        }

        check = scriptRepository.Check();
        check.Should().BeFalse();
        
        scriptRepository.Create();
        
        check = scriptRepository.Check();
        check.Should().BeTrue();
    }
    
    [Test]
    public void Should_get_all_scripts_in_a_lookup()
    {
        var scriptRepository = _container.GetInstance<IScriptRepository>();
        
        var scripts = scriptRepository.GetScripts();

        scripts.Should().NotBeNull();
    }
    
    [Test]
    public void Should_insert_script()
    {
        var scriptRepository = _container.GetInstance<IScriptRepository>();

        var script = new Script
        {
            RepositoryPath = "path/to/some.sql",
            FileName = "some.sql",
            Status = ScriptStatus.Processed,
            Hash = "123",
        };
        
        scriptRepository.Insert(script);
    }
    
    [Test]
    public void Should_update_script()
    {
        var scriptRepository = _container.GetInstance<IScriptRepository>();

        var script = new Script
        {
            Id = 1,
            Status = ScriptStatus.Failed,
            Hash = "123",
            ErrorMessage = "some error message"
        };
        
        scriptRepository.Update(script);
    }
    
    [Test]
    public void Should_apply_a_script()
    {
        var scriptRepository = _container.GetInstance<IScriptRepository>();
        
        scriptRepository.Apply("select current_timestamp from dual;");
    }
    
    [Test]
    public void Should_save_state()
    {
        var scriptRepository = _container.GetInstance<IStateRepository>();

        scriptRepository.Update("1234");
    }
    
    [Test]
    public void Should_get_state()
    {
        var scriptRepository = _container.GetInstance<IStateRepository>();

        scriptRepository.Update("1234");

        var state = scriptRepository.Get();

        state.Should().Be("1234");
    }
}