using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;



try
{
    await using (var connection = new SqlConnection("Data Source=localhost;User ID=sa;Password=MasadNetunim12!@;Initial Catalog=master;TrustServerCertificate=true;MultipleActiveResultSets=true;"))
    {
        await connection.OpenAsync();

        await DropTableAsync(connection);
        await DropTableTypeAsync(connection);

        await CreateTableTypeAsync(connection);
        await CreateTableAsync(connection);

        var data = new List<MyRecord>
        {
            new MyRecord(1, "John", 5),
            new MyRecord(2, "Alice", 3),
            new MyRecord(3, "Bob", 8)
        };

        await InsertDataAsync(connection, data);
    }
}
finally
{
    await using (var connection = new SqlConnection("Data Source=localhost;User ID=sa;Password=MasadNetunim12!@;Initial Catalog=master;TrustServerCertificate=true;MultipleActiveResultSets=true;"))
    {
        await connection.OpenAsync();
        await DropTableAsync(connection);
        await DropTableTypeAsync(connection);
    }
}

static async Task CreateTableTypeAsync(SqlConnection connection)
{
    using var command = connection.CreateCommand();
    command.CommandText = "CREATE TYPE MyTableType AS TABLE (Id INT, Name NVARCHAR(255), Rank INT)";
    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
}

static async Task CreateTableAsync(SqlConnection connection)
{
    using var command = connection.CreateCommand();
    command.CommandText = "CREATE TABLE MyTable (Id INT, Name NVARCHAR(255), Rank INT)";
    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
}

static async Task InsertDataAsync(SqlConnection connection, IEnumerable<MyRecord> data)
{
    using var tvp = new TableValuedParameter("MyTableType");
    foreach (var item in data)
    {
        tvp.AddRow(item.Id, item.Name, item.Rank);
    }

    using var command = connection.CreateCommand();
    command.CommandText = "INSERT INTO MyTable (Id, Name, Rank) SELECT Id, Name, Rank FROM @MyTableType";
    command.Parameters.AddWithValue("@MyTableType", tvp).SqlDbType = SqlDbType.Structured;
    command.Parameters["@MyTableType"].TypeName = "MyTableType";

    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
}

static async Task DropTableAsync(SqlConnection connection)
{
    using var command = connection.CreateCommand();
    command.CommandText = "IF OBJECT_ID('MyTable', 'U') IS NOT NULL DROP TABLE MyTable";
    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
}

static async Task DropTableTypeAsync(SqlConnection connection)
{
    using var command = connection.CreateCommand();
    command.CommandText = "IF EXISTS (SELECT 1 FROM sys.types WHERE name = 'MyTableType') DROP TYPE MyTableType";
    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
}

public class TableValuedParameter : List<SqlDataRecord>, IDisposable
{
    private readonly SqlMetaData[] _metadata;
    private readonly SqlDataRecord _record;

    public TableValuedParameter(string typeName)
    {
        _metadata = new[]
        {
            new SqlMetaData("Id", SqlDbType.Int),
            new SqlMetaData("Name", SqlDbType.NVarChar, 255),
            new SqlMetaData("Rank", SqlDbType.Int)
        };

        _record = new SqlDataRecord(_metadata);
    }

    public void AddRow(int id, string name, int rank)
    {
        _record.SetInt32(0, id);
        _record.SetString(1, name);
        _record.SetInt32(2, rank);

        Add(_record);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
record MyRecord(int Id, string Name, int Rank);