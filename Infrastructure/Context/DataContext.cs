using System.Data;
using Npgsql;

namespace Infrastructure.Context;

public class DataContext
{
    private readonly string connectionString =
        "Host = localhost ; Database = exam1_cs3 ; Username = postgres ; Port = 5432 ; Password = mai2026";

    public IDbConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        return connection;
    }
}