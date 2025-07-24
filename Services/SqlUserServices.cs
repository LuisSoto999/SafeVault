using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

public class SqlUserService
{
    private readonly string _connectionString;

    public SqlUserService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public void AddUser(string username, string passwordHash, string role)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Users (Username, PasswordHash, Role) VALUES (@username, @passwordHash, @role)";
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@passwordHash", passwordHash);
        command.Parameters.AddWithValue("@role", role);

        command.ExecuteNonQuery();
    }

    public User GetByUsername(string username)
    {
        using var conn = new MySqlConnection(_connectionString); // o SqlConnection directa
        conn.Open();

        using var command = conn.CreateCommand();
        command.CommandText = "SELECT Username, PasswordHash, Role FROM Users WHERE Username = @username";
        command.Parameters.AddWithValue("@username", username);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Username = reader.GetString(0),
                PasswordHash = reader.GetString(1),
                Role = reader.GetString(2)
            };
        }

        return null; // Not found
    }
}