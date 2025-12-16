using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MiniBanque.Grpc.Model
{
    public class Transactions
    {
        private readonly string _connectionString;

        public Transactions(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> RecordAsync(
            MySqlConnection conn,
            int sourceAccount,
            string type,
            double amount,
            string description,
            int? destinationAccount,
            MySqlTransaction transaction = null)
        {
            var query = @"INSERT INTO transaction 
                (id_compte_source, typetransaction, montant, description, id_compte_destination)
                VALUES (@src, @type, @amount, @desc, @dest);
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@src", sourceAccount);
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@amount", amount);
            cmd.Parameters.AddWithValue("@desc", description);
            cmd.Parameters.AddWithValue("@dest", destinationAccount ?? (object)DBNull.Value);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<List<(int Id, string Type, double Amount, DateTime Date, string Desc, int Src, int? Dest)>>
            GetByAccountAsync(int accountId, int limit)
        {
            var list = new List<(int, string, double, DateTime, string, int, int?)>();

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = @"SELECT * FROM transaction 
                          WHERE id_compte_source = @id OR id_compte_destination = @id
                          ORDER BY date_transaction DESC
                          LIMIT @limit";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", accountId);
            cmd.Parameters.AddWithValue("@limit", limit > 0 ? limit : 50);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add((
                    reader.GetInt32("id_transaction"),
                    reader.GetString("typetransaction"),
                    (double)reader.GetDecimal("montant"),
                    reader.GetDateTime("date_transaction"),
                    reader.IsDBNull("description") ? "" : reader.GetString("description"),
                    reader.GetInt32("id_compte_source"),
                    reader.IsDBNull("id_compte_destination") ? null : reader.GetInt32("id_compte_destination")
                ));
            }

            return list;
        }
    }
}
