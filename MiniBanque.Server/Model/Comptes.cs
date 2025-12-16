using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MiniBanque.Grpc.Model
{
    public class Comptes
    {
        private readonly string _connectionString;

        public Comptes(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> CreateAsync(int userId, string type, double initialDeposit)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = @"INSERT INTO comptes (id_client, type, solde) 
                          VALUES (@id_client, @type, @solde);
                          SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id_client", userId);
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@solde", initialDeposit);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<(double Balance, string Type)> GetBalanceAsync(int accountId)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = "SELECT solde, type FROM comptes WHERE id_compte = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", accountId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return (
                    (double)reader.GetDecimal("solde"),
                    reader.GetString("type")
                );
            }

            return (0, null);
        }

        public async Task<List<(int Id, string Type, double Solde)>> GetByUserAsync(int userId)
        {
            var result = new List<(int, string, double)>();

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = "SELECT id_compte, type, solde FROM comptes WHERE id_client = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add((
                    reader.GetInt32("id_compte"),
                    reader.GetString("type"),
                    (double)reader.GetDecimal("solde")
                ));
            }

            return result;
        }
    }
}
