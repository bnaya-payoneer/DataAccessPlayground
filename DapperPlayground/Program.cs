using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;


DatabaseType databaseType = DatabaseType.SqlServer;

string connectionString = databaseType switch
{
    DatabaseType.SqlServer => "Data Source=localhost;User ID=sa;Password=MasadNetunim12!@;Initial Catalog=master;TrustServerCertificate=true;MultipleActiveResultSets=true;",
    DatabaseType.MySql => "YourMySqlConnectionString",
    DatabaseType.Postgres => "YourPostgresConnectionString",
    DatabaseType.Oracle => "YourOracleConnectionString",
    _ => throw new ArgumentOutOfRangeException(nameof(databaseType), "Unsupported database type"),
};

DbConnection connection = databaseType switch
{
    DatabaseType.SqlServer => new SqlConnection(connectionString),
    DatabaseType.MySql => new MySqlConnection(connectionString),
    DatabaseType.Postgres => new NpgsqlConnection(connectionString),
    DatabaseType.Oracle => new OracleConnection(connectionString),
    _ => throw new ArgumentOutOfRangeException(nameof(databaseType), "Unsupported database type"),
};

await connection.OpenAsync();

await connection.ExecuteAsync("IF OBJECT_ID('MyTable', 'U') IS NOT NULL DROP TABLE MyTable");
await connection.ExecuteAsync("IF EXISTS (SELECT 1 FROM sys.types WHERE name = 'MyTableType') DROP TYPE MyTableType");

await connection.ExecuteAsync("CREATE TYPE MyTableType AS TABLE (Id INT, Name NVARCHAR(255), Rank INT)");
await connection.ExecuteAsync("CREATE TABLE MyTable (Id INT, Name NVARCHAR(255), Rank INT)");

var data = new List<MyRecord>
{
    new MyRecord(1, "John", 5),
    new MyRecord(2, "Alice", 3),
    new MyRecord(3, "Bob", 8)
};

int affected = await connection.ExecuteAsync(
    "INSERT INTO MyTable (Id, Name, Rank) VALUES (@Id, @Name, @Rank)",
    data
);

Console.WriteLine($"Affected = {affected}");

record MyRecord(int Id, string Name, int Rank);

public enum DatabaseType
{
    SqlServer,
    MySql,
    Postgres,
    Oracle
}


