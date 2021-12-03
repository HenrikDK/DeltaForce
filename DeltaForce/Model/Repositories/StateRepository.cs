using DeltaForce.Infrastructure;
using DeltaForce.Model.Dialects;

namespace DeltaForce.Model.Repositories;

public interface IStateRepository
{
    string Get();
    void Update(string commitHash);
}
    
public class StateRepository : IStateRepository
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ISqlDialect _dialect;

    public StateRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        _dialect = _connectionFactory.GetDialect();
    }

    public string Get()
    {
        var sql = _dialect.GetState;
        using var connection = _connectionFactory.Get();
        return connection.Query<string>(sql).FirstOrDefault();
    }

    public void Update(string hash)
    {
        var sql = _dialect.SaveState;
        using var connection = _connectionFactory.Get();
        connection.Execute(sql, new {hash});
    }
}