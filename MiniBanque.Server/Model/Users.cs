using MySql.Data.MySqlClient;
using System.Data;
using System.Security.Cryptography;
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

        public async Task<bool> UsernameExistsAsync(string username)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            const string query = @"SELECT COUNT(1) FROM users WHERE username = @username";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string mail)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            const string query = @"SELECT COUNT(1) FROM users WHERE mail = @mail";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@mail", mail);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<(bool Success, int UserId, string Message)> RegisterAsync(
            string nom,
            string prenom,
            string email,
            string username,
            string password,
            string role = "client")
        {
            // Hachage du mot de passe
            var plainPwd = password;

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            // Insertion utilisateur
            const string insertSql = @"
INSERT INTO users (nom, prenom, mail, username, password, role)
VALUES (@nom, @prenom, @mail, @username, @password, @role);
SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(insertSql, conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@mail", email);
            cmd.Parameters.AddWithValue("@nom", nom);
            cmd.Parameters.AddWithValue("@prenom", prenom);
            cmd.Parameters.AddWithValue("@password", plainPwd);
            cmd.Parameters.AddWithValue("@role", role);

            try
            {
                var idObj = await cmd.ExecuteScalarAsync();
                var newId = Convert.ToInt32(idObj);
                return (true, newId, "Utilisateur créé");
            }
            catch (MySqlException ex) when (ex.Number == 1062) // duplicate key
            {
                // Cas collision d'unicité si contraintes uniques côté DB
                return (false, 0, "Nom d'utilisateur ou email déjà utilisé");
            }
            catch (Exception ex)
            {
                return (false, 0, $"Erreur lors de la création: {ex.Message}");
            }
        }

        private static string HashPassword(string password)
        {
            const int iterations = 100_000;
            const int saltSize = 16;
            const int keySize = 32;

            Span<byte> salt = stackalloc byte[saltSize];
            RandomNumberGenerator.Fill(salt);

            Span<byte> key = stackalloc byte[keySize];
            Rfc2898DeriveBytes.Pbkdf2(
                password.AsSpan(),
                salt,
                key,
                iterations,
                HashAlgorithmName.SHA256);

            var saltB64 = Convert.ToBase64String(salt);
            var keyB64 = Convert.ToBase64String(key);
            return $"PBKDF2${iterations}${saltB64}${keyB64}";
        }

        private static bool VerifyPassword(string password, string stored)
        {
            // Compatibilité: si ancien stockage en clair (pas de $), faire comparaison directe
            if (string.IsNullOrEmpty(stored) || !stored.Contains('$'))
            {
                return string.Equals(password, stored, StringComparison.Ordinal);
            }

            // Format attendu: PBKDF2$<iterations>$<salt>$<hash>
            var parts = stored.Split('$');
            if (parts.Length != 4 || !string.Equals(parts[0], "PBKDF2", StringComparison.Ordinal))
            {
                return false;
            }

            if (!int.TryParse(parts[1], out var iterations))
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);

            Span<byte> computed = stackalloc byte[expected.Length];
            Rfc2898DeriveBytes.Pbkdf2(
                password.AsSpan(),
                salt,
                computed,
                iterations,
                HashAlgorithmName.SHA256);

            return CryptographicOperations.FixedTimeEquals(computed, expected);
        }
    }
}
