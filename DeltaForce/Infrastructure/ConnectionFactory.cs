using System.Data.SqlClient;
using DeltaForce.Model.Dialects;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace DeltaForce.Infrastructure;

public interface IConnectionFactory
{
    IDbConnection Get();
    ISqlDialect GetDialect();
}
    
public class ConnectionFactory : IConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly Lazy<string> _connectionString;
    private readonly Lazy<string> _dbType;

    public ConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
        _dbType = new Lazy<string>(() => _configuration.GetValue<string>("database-type"));
        _connectionString = new Lazy<string>(() => _configuration.GetValue<string>("connection-string"));
    }

    public IDbConnection Get()
    {
        var connection = GetConnection();
        connection.Open();
        return connection;
    }

    public ISqlDialect GetDialect()
    {
        return _dbType.Value switch
        {
            "postgresql" => new NpgSqlDialect(),
            "oracle" => new OracleSqlDialect(),
            "mysql" => new MySqlDialect(),
            "mssql" => new MsSqlDialect(),
            _ => null
        };
    }

    private IDbConnection GetConnection()
    {
        return _dbType.Value switch
        {
            "postgresql" => new NpgsqlConnection(_connectionString.Value),
            "oracle" => new OracleConnection(_connectionString.Value),
            "mysql" => new MySqlConnection(_connectionString.Value),
            "mssql" => new SqlConnection(_connectionString.Value),
            _ => null
        };
    }
}