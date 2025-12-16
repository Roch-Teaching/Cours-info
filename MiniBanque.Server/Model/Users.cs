using MySql.Data.MySqlClient;
using System.Data;
using System.Threading.Tasks;

namespace MiniBanque.Grpc.Model
{
    public class Users
    {
        private readonly string _connectionString;

        public Users(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<(bool Success, int Id, string Role, string Nom, string Prenom)> LoginAsync(string username, string password)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = @"SELECT id_user, role, nom, prenom, password 
                          FROM users 
                          WHERE username = @username";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var storedPassword = reader.GetString("password");

                if (storedPassword == password)
                {
                    return (
                        true,
                        reader.GetInt32("id_user"),
                        reader.GetString("role"),
                        reader.GetString("nom"),
                        reader.GetString("prenom")
                    );
                }
            }

            return (false, 0, null, null, null);
        }
    }
}
