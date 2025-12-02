using Grpc.Core;
using Microsoft.AspNetCore.Identity.Data;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Transactions;

namespace MiniBanque.Grpc.Services
{
    public class BankServiceImpl : BankService.BankServiceBase
    {
        private readonly string _connectionString;

        public BankServiceImpl(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Authentification
        public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = "SELECT id_user, role, nom, prenom, password FROM users WHERE username = @username";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", request.Username);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var storedPassword = reader.GetString("password");

                // Vérification du mot de passe (ajoutez du hashing en production!)
                if (storedPassword == request.Password)
                {
                    return new LoginResponse
                    {
                        Success = true,
                        Message = "Connexion réussie",
                        UserId = reader.GetInt32("id_user"),
                        Role = reader.GetString("role"),
                        Nom = reader.GetString("nom"),
                        Prenom = reader.GetString("prenom")
                    };
                }
            }

            return new LoginResponse
            {
                Success = false,
                Message = "Nom d'utilisateur ou mot de passe incorrect"
            };
        }

        // Créer un compte
        public override async Task<AccountResponse> CreateAccount(CreateAccountRequest request, ServerCallContext context)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = "INSERT INTO comptes (id_client, type, solde) VALUES (@id_client, @type, @solde); SELECT LAST_INSERT_ID();";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id_client", request.UserId);
            cmd.Parameters.AddWithValue("@type", request.Type);
            cmd.Parameters.AddWithValue("@solde", request.InitialDeposit);

            var accountId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            // Enregistrer la transaction initiale si dépôt initial
            if (request.InitialDeposit > 0)
            {
                await RecordTransaction(conn, accountId, "depot", request.InitialDeposit, "Dépôt initial", null);
            }

            return new AccountResponse
            {
                Success = true,
                Message = "Compte créé avec succès",
                AccountId = accountId,
                Balance = request.InitialDeposit
            };
        }

        // Obtenir les comptes d'un utilisateur
        public override async Task<GetUserAccountsResponse> GetUserAccounts(GetUserAccountsRequest request, ServerCallContext context)
        {
            var response = new GetUserAccountsResponse { Success = true };

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = "SELECT id_compte, type, solde FROM comptes WHERE id_client = @id_client";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id_client", request.UserId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                response.Accounts.Add(new Account
                {
                    IdCompte = reader.GetInt32("id_compte"),
                    Type = reader.GetString("type"),
                    Solde = (double)reader.GetDecimal("solde")
                });
            }

            return response;
        }

        // Obtenir le solde d'un compte
        public override async Task<BalanceResponse> GetBalance(GetBalanceRequest request, ServerCallContext context)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = "SELECT solde, type FROM comptes WHERE id_compte = @id_compte";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id_compte", request.AccountId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new BalanceResponse
                {
                    Success = true,
                    Balance = (double)reader.GetDecimal("solde"),
                    AccountType = reader.GetString("type")
                };
            }

            return new BalanceResponse { Success = false };
        }

        // Dépôt d'argent
        public override async Task<TransactionResponse> Deposit(DepositRequest request, ServerCallContext context)
        {
            if (request.Amount <= 0)
            {
                return new TransactionResponse
                {
                    Success = false,
                    Message = "Le montant doit être positif"
                };
            }

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Mettre à jour le solde
                var updateQuery = "UPDATE comptes SET solde = solde + @montant WHERE id_compte = @id_compte";
                using var updateCmd = new MySqlCommand(updateQuery, conn, transaction);
                updateCmd.Parameters.AddWithValue("@montant", request.Amount);
                updateCmd.Parameters.AddWithValue("@id_compte", request.AccountId);
                await updateCmd.ExecuteNonQueryAsync();

                // Enregistrer la transaction
                var transId = await RecordTransaction(conn, request.AccountId, "depot", request.Amount,
                    request.Description ?? "Dépôt", null, transaction);

                // Obtenir le nouveau solde
                var balanceQuery = "SELECT solde FROM comptes WHERE id_compte = @id_compte";
                using var balanceCmd = new MySqlCommand(balanceQuery, conn, transaction);
                balanceCmd.Parameters.AddWithValue("@id_compte", request.AccountId);
                var newBalance = (double)Convert.ToDecimal(await balanceCmd.ExecuteScalarAsync());

                await transaction.CommitAsync();

                return new TransactionResponse
                {
                    Success = true,
                    Message = "Dépôt effectué avec succès",
                    TransactionId = transId,
                    NewBalance = newBalance
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new TransactionResponse
                {
                    Success = false,
                    Message = $"Erreur: {ex.Message}"
                };
            }
        }

        // Virement
        public override async Task<TransactionResponse> Transfer(TransferRequest request, ServerCallContext context)
        {
            if (request.Amount <= 0)
            {
                return new TransactionResponse
                {
                    Success = false,
                    Message = "Le montant doit être positif"
                };
            }

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Vérifier le solde du compte source
                var checkQuery = "SELECT solde FROM comptes WHERE id_compte = @id_compte";
                using var checkCmd = new MySqlCommand(checkQuery, conn, transaction);
                checkCmd.Parameters.AddWithValue("@id_compte", request.SourceAccountId);
                var balance = Convert.ToDecimal(await checkCmd.ExecuteScalarAsync());

                if (balance < (decimal)request.Amount)
                {
                    await transaction.RollbackAsync();
                    return new TransactionResponse
                    {
                        Success = false,
                        Message = "Solde insuffisant"
                    };
                }

                // Débiter le compte source
                var debitQuery = "UPDATE comptes SET solde = solde - @montant WHERE id_compte = @id_compte";
                using var debitCmd = new MySqlCommand(debitQuery, conn, transaction);
                debitCmd.Parameters.AddWithValue("@montant", request.Amount);
                debitCmd.Parameters.AddWithValue("@id_compte", request.SourceAccountId);
                await debitCmd.ExecuteNonQueryAsync();

                // Créditer le compte destination
                var creditQuery = "UPDATE comptes SET solde = solde + @montant WHERE id_compte = @id_compte";
                using var creditCmd = new MySqlCommand(creditQuery, conn, transaction);
                creditCmd.Parameters.AddWithValue("@montant", request.Amount);
                creditCmd.Parameters.AddWithValue("@id_compte", request.DestinationAccountId);
                await creditCmd.ExecuteNonQueryAsync();

                // Enregistrer la transaction
                var transId = await RecordTransaction(conn, request.SourceAccountId, "virement",
                    request.Amount, request.Description ?? "Virement", request.DestinationAccountId, transaction);

                // Obtenir le nouveau solde
                var balanceQuery = "SELECT solde FROM comptes WHERE id_compte = @id_compte";
                using var balanceCmd = new MySqlCommand(balanceQuery, conn, transaction);
                balanceCmd.Parameters.AddWithValue("@id_compte", request.SourceAccountId);
                var newBalance = (double)Convert.ToDecimal(await balanceCmd.ExecuteScalarAsync());

                await transaction.CommitAsync();

                return new TransactionResponse
                {
                    Success = true,
                    Message = "Virement effectué avec succès",
                    TransactionId = transId,
                    NewBalance = newBalance
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new TransactionResponse
                {
                    Success = false,
                    Message = $"Erreur: {ex.Message}"
                };
            }
        }

        // Obtenir l'historique des transactions
        public override async Task<GetTransactionsResponse> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {
            var response = new GetTransactionsResponse { Success = true };

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = @"SELECT id_transaction, typetransaction, montant, date_transaction, 
                         description, id_compte_source, id_compte_destination 
                         FROM transaction 
                         WHERE id_compte_source = @id_compte OR id_compte_destination = @id_compte
                         ORDER BY date_transaction DESC 
                         LIMIT @limit";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id_compte", request.AccountId);
            cmd.Parameters.AddWithValue("@limit", request.Limit > 0 ? request.Limit : 50);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                response.Transactions.Add(new Transaction
                {
                    IdTransaction = reader.GetInt32("id_transaction"),
                    TypeTransaction = reader.GetString("typetransaction"),
                    Montant = (double)reader.GetDecimal("montant"),
                    DateTransaction = reader.GetDateTime("date_transaction").ToString("yyyy-MM-dd HH:mm:ss"),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                    IdCompteSource = reader.GetInt32("id_compte_source"),
                    IdCompteDestination = reader.IsDBNull(reader.GetOrdinal("id_compte_destination"))
                        ? 0 : reader.GetInt32("id_compte_destination")
                });
            }

            return response;
        }

        // Calculer les intérêts (pour comptes épargne)
        public override async Task<InterestResponse> CalculateInterest(CalculateInterestRequest request, ServerCallContext context)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            // Vérifier que c'est un compte épargne
            var checkQuery = "SELECT type, solde FROM comptes WHERE id_compte = @id_compte";
            using var checkCmd = new MySqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@id_compte", request.AccountId);

            using var reader = await checkCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return new InterestResponse
                {
                    Success = false,
                    Message = "Compte introuvable"
                };
            }

            var accountType = reader.GetString("type");
            var balance = (double)reader.GetDecimal("solde");
            reader.Close();

            if (accountType != "epargne" && accountType != "epragne")
            {
                return new InterestResponse
                {
                    Success = false,
                    Message = "Les intérêts ne sont calculés que pour les comptes épargne"
                };
            }

            // Taux d'intérêt annuel de 3%
            var interestRate = 0.03;
            var interestAmount = balance * interestRate;

            using var transaction = await conn.BeginTransactionAsync();
            try
            {
                // Ajouter les intérêts au solde
                var updateQuery = "UPDATE comptes SET solde = solde + @interets WHERE id_compte = @id_compte";
                using var updateCmd = new MySqlCommand(updateQuery, conn, transaction);
                updateCmd.Parameters.AddWithValue("@interets", interestAmount);
                updateCmd.Parameters.AddWithValue("@id_compte", request.AccountId);
                await updateCmd.ExecuteNonQueryAsync();

                // Enregistrer la transaction
                await RecordTransaction(conn, request.AccountId, "interets", interestAmount,
                    "Intérêts annuels", null, transaction);

                await transaction.CommitAsync();

                return new InterestResponse
                {
                    Success = true,
                    Message = "Intérêts calculés et ajoutés avec succès",
                    InterestAmount = interestAmount,
                    NewBalance = balance + interestAmount,
                    InterestRate = interestRate * 100
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new InterestResponse
                {
                    Success = false,
                    Message = $"Erreur: {ex.Message}"
                };
            }
        }

        // Méthode utilitaire pour enregistrer une transaction
        private async Task<int> RecordTransaction(MySqlConnection conn, int sourceAccountId, string type,
            double amount, string description, int? destinationAccountId, MySqlTransaction transaction = null)
        {
            var query = @"INSERT INTO transaction (id_compte_source, typetransaction, montant, description, id_compte_destination) 
                         VALUES (@source, @type, @montant, @desc, @dest); 
                         SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@source", sourceAccountId);
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@montant", amount);
            cmd.Parameters.AddWithValue("@desc", description);
            cmd.Parameters.AddWithValue("@dest", destinationAccountId.HasValue ? (object)destinationAccountId.Value : DBNull.Value);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
    }
}