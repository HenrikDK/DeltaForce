using DeltaForce.Infrastructure;
using DeltaForce.Model.Dialects;

namespace DeltaForce.Model.Repositories;

public interface IScriptRepository
{
    bool Check();
    void Create();
    ILookup<string, Script> GetScripts();
    void Insert(Script script);
    void Update(Script script);
    void Apply(string script);
}

public class ScriptRepository : IScriptRepository
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ISqlDialect _dialect;

    public ScriptRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        _dialect = connectionFactory.GetDialect();
    }
    
    public bool Check()
    {
        var sql = _dialect.CheckSchema;
        using var connection = _connectionFactory.Get();
        return connection.Query<bool>(sql).FirstOrDefault();
    }
    
    public void Create()
    {
        var sql = _dialect.CreateSchema;
        using var connection = _connectionFactory.Get();
        connection.Execute(sql);
    }

    public ILookup<string, Script> GetScripts()
    {
        var sql = _dialect.GetScripts;
        using var connection = _connectionFactory.Get();
        return connection.Query<Script>(sql).ToLookup(x => x.RepositoryPath);
    }

    public void Insert(Script script)
    {
        var sql = _dialect.InsertScript;
        using var connection = _connectionFactory.Get();
        connection.Execute(sql, script);
    }

    public void Update(Script script)
    {
        var sql = _dialect.UpdateScript;
        using var connection = _connectionFactory.Get();
        connection.Execute(sql, script);
    }

    public void Apply(string script)
    {
        using var connection = _connectionFactory.Get();
        connection.Execute(script);
    }
}