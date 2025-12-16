using Grpc.Core;
using MiniBanque.Grpc;
using MiniBanque.Grpc.Model;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;


namespace MiniBanque.Grpc.Services
{
    public class BankServiceImpl : BankServiceBase
    {
        private readonly string _connectionString;

        private readonly Users _users;
        private readonly Comptes _comptes;
        private readonly Transactions _transactions;

        public BankServiceImpl(string connectionString)
        {
            _connectionString = connectionString;

            _users = new Users(_connectionString);
            _comptes = new Comptes(_connectionString);
            _transactions = new Transactions(_connectionString);
        }

        // ===================== AUTH =====================
        public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            var result = await _users.LoginAsync(request.Username, request.Password);

            if (!result.Success)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Nom d'utilisateur ou mot de passe incorrect"
                };
            }

            return new LoginResponse
            {
                Success = true,
                Message = "Connexion réussie",
                UserId = result.Id,
                Role = result.Role,
                Nom = result.Nom,
                Prenom = result.Prenom
            };
        }

        // ===================== COMPTES =====================
        public override async Task<AccountResponse> CreateAccount(CreateAccountRequest request, ServerCallContext context)
        {
            var accountId = await _comptes.CreateAsync(
                request.UserId,
                request.Type,
                request.InitialDeposit
            );

            if (request.InitialDeposit > 0)
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                await _transactions.RecordAsync(
                    conn,
                    accountId,
                    "depot",
                    request.InitialDeposit,
                    "Dépôt initial",
                    null
                );
            }

            return new AccountResponse
            {
                Success = true,
                Message = "Compte créé avec succès",
                AccountId = accountId,
                Balance = request.InitialDeposit
            };
        }

        public override async Task<GetUserAccountsResponse> GetUserAccounts(GetUserAccountsRequest request, ServerCallContext context)
        {
            var response = new GetUserAccountsResponse { Success = true };

            var comptes = await _comptes.GetByUserAsync(request.UserId);

            foreach (var c in comptes)
            {
                response.Accounts.Add(new Account
                {
                    IdCompte = c.Id,
                    Type = c.Type,
                    Solde = c.Solde
                });
            }

            return response;
        }

        public override async Task<BalanceResponse> GetBalance(GetBalanceRequest request, ServerCallContext context)
        {
            var (balance, type) = await _comptes.GetBalanceAsync(request.AccountId);

            if (type == null)
            {
                return new BalanceResponse { Success = false };
            }

            return new BalanceResponse
            {
                Success = true,
                Balance = balance,
                AccountType = type
            };
        }

        // ===================== DEPOT =====================
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
            using var dbTransaction = await conn.BeginTransactionAsync();

            try
            {
                var updateCmd = new MySqlCommand(
                    "UPDATE comptes SET solde = solde + @m WHERE id_compte = @id",
                    conn,
                    dbTransaction);

                updateCmd.Parameters.AddWithValue("@m", request.Amount);
                updateCmd.Parameters.AddWithValue("@id", request.AccountId);
                await updateCmd.ExecuteNonQueryAsync();

                var transId = await _transactions.RecordAsync(
                    conn,
                    request.AccountId,
                    "depot",
                    request.Amount,
                    request.Description ?? "Dépôt",
                    null,
                    dbTransaction
                );

                var balanceCmd = new MySqlCommand(
                    "SELECT solde FROM comptes WHERE id_compte = @id",
                    conn,
                    dbTransaction);

                balanceCmd.Parameters.AddWithValue("@id", request.AccountId);
                var newBalance = Convert.ToDouble(await balanceCmd.ExecuteScalarAsync());

                await dbTransaction.CommitAsync();

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
                await dbTransaction.RollbackAsync();
                return new TransactionResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        // ===================== VIREMENT =====================
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
            using var dbTransaction = await conn.BeginTransactionAsync();

            try
            {
                // Vérification du solde du compte source
                var checkCmd = new MySqlCommand(
                    "SELECT solde FROM comptes WHERE id_compte = @id",
                    conn,
                    dbTransaction);

                checkCmd.Parameters.AddWithValue("@id", request.SourceAccountId);
                var balance = Convert.ToDecimal(await checkCmd.ExecuteScalarAsync());

                if (balance < (decimal)request.Amount)
                {
                    await dbTransaction.RollbackAsync();
                    return new TransactionResponse
                    {
                        Success = false,
                        Message = "Solde insuffisant"
                    };
                }

                // Vérification de l'existence du compte destinataire
                var destCheckCmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM comptes WHERE id_compte = @id",
                    conn,
                    dbTransaction);

                destCheckCmd.Parameters.AddWithValue("@id", request.DestinationAccountId);
                var exists = Convert.ToInt32(await destCheckCmd.ExecuteScalarAsync());

                if (exists == 0)
                {
                    // Annuler la transaction si le compte destinataire n'existe pas
                    await dbTransaction.RollbackAsync();
                    return new TransactionResponse
                    {
                        Success = false,
                        Message = "Compte destinataire introuvable"
                    };
                }

                // Mise à jour du solde du compte source
                await new MySqlCommand(
                    "UPDATE comptes SET solde = solde - @m WHERE id_compte = @id",
                    conn,
                    dbTransaction)
                {
                    Parameters =
            {
                new MySqlParameter("@m", request.Amount),
                new MySqlParameter("@id", request.SourceAccountId)
            }
                }.ExecuteNonQueryAsync();

                // Mise à jour du solde du compte destinataire
                await new MySqlCommand(
                    "UPDATE comptes SET solde = solde + @m WHERE id_compte = @id",
                    conn,
                    dbTransaction)
                {
                    Parameters =
            {
                new MySqlParameter("@m", request.Amount),
                new MySqlParameter("@id", request.DestinationAccountId)
            }
                }.ExecuteNonQueryAsync();

                // Enregistrement de la transaction
                var transId = await _transactions.RecordAsync(
                    conn,
                    request.SourceAccountId,
                    "virement",
                    request.Amount,
                    request.Description ?? "Virement",
                    request.DestinationAccountId,
                    dbTransaction
                );

                // Récupérer le nouveau solde du compte source
                var balanceCmd = new MySqlCommand(
                    "SELECT solde FROM comptes WHERE id_compte = @id",
                    conn,
                    dbTransaction);

                balanceCmd.Parameters.AddWithValue("@id", request.SourceAccountId);
                var newBalance = Convert.ToDouble(await balanceCmd.ExecuteScalarAsync());

                await dbTransaction.CommitAsync();

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
                await dbTransaction.RollbackAsync();
                return new TransactionResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }


        // ===================== HISTORIQUE =====================
        public override async Task<GetTransactionsResponse> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {
            var response = new GetTransactionsResponse { Success = true };

            var list = await _transactions.GetByAccountAsync(request.AccountId, request.Limit);

            foreach (var t in list)
            {
                response.Transactions.Add(new Transaction
                {
                    IdTransaction = t.Id,
                    TypeTransaction = t.Type,
                    Montant = t.Amount,
                    DateTransaction = t.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                    Description = t.Desc,
                    IdCompteSource = t.Src,
                    IdCompteDestination = t.Dest ?? 0
                });
            }

            return response;
        }

        // ===================== INTERETS =====================
        public override async Task<InterestResponse> CalculateInterest(
            CalculateInterestRequest request,
            ServerCallContext context)
        {
            var (balance, type) = await _comptes.GetBalanceAsync(request.AccountId);

            if (type == null)
            {
                return new InterestResponse
                {
                    Success = false,
                    Message = "Compte introuvable"
                };
            }

            if (type != "epargne")
            {
                return new InterestResponse
                {
                    Success = false,
                    Message = "Les intérêts ne sont calculés que pour les comptes épargne"
                };
            }

            const double interestRate = 0.03;
            var interestAmount = balance * interestRate;
            var newBalance = balance + interestAmount;

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var dbTransaction = await conn.BeginTransactionAsync();

            try
            {
                var updateCmd = new MySqlCommand(
                    "UPDATE comptes SET solde = solde + @i WHERE id_compte = @id",
                    conn,
                    dbTransaction);

                updateCmd.Parameters.AddWithValue("@i", interestAmount);
                updateCmd.Parameters.AddWithValue("@id", request.AccountId);
                await updateCmd.ExecuteNonQueryAsync();

                await _transactions.RecordAsync(
                    conn,
                    request.AccountId,
                    "interets",
                    interestAmount,
                    "Intérêts annuels",
                    null,
                    dbTransaction
                );

                await dbTransaction.CommitAsync();

                return new InterestResponse
                {
                    Success = true,
                    Message = "Intérêts calculés avec succès",
                    InterestAmount = interestAmount,
                    NewBalance = newBalance,
                    InterestRate = interestRate * 100
                };
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return new InterestResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
