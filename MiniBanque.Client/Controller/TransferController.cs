using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using MiniBanque.Grpc;
using Grpc.Core;

namespace MiniBanque.Client.Controller;

/// <summary>
/// Contrôleur dédié à la gestion des virements et comptes bancaires
/// </summary>
class TransferController
{
    /// <summary>
    /// Récupère tous les comptes bancaires de l'utilisateur connecté
    /// </summary>
    public static async Task<List<Account>> GetBankAccountsAsync()
    {
        if (Gestion.CurrentUserId is null)
        {
            throw new InvalidOperationException("Aucun utilisateur connecté. Veuillez vous reconnecter.");
        }

        using var channel = GrpcChannel.ForAddress(Gestion.ServerAddress);
        var client = new BankService.BankServiceClient(channel);

        var response = await client.GetUserAccountsAsync(new GetUserAccountsRequest
        {
            UserId = Gestion.CurrentUserId.Value
        });

        return response.Accounts.ToList();
    }

    /// <summary>
    /// Effectue un virement d'un compte vers un autre
    /// </summary>
    /// <param name="sourceAccountId">ID du compte à débiter</param>
    /// <param name="destinationAccountId">ID du compte destinataire</param>
    /// <param name="amount">Montant du virement</param>
    /// <param name="description">Motif du virement</param>
    /// <returns>TransactionResponse avec le résultat de l'opération</returns>
    public static async Task<TransactionResponse> MakeTransferAsync(
        int sourceAccountId,
        int destinationAccountId,
        double amount,
        string description)
    {
        if (Gestion.CurrentUserId is null)
        {
            throw new InvalidOperationException("Aucun utilisateur connecté. Veuillez vous reconnecter.");
        }

        using var channel = GrpcChannel.ForAddress(Gestion.ServerAddress);
        var client = new BankService.BankServiceClient(channel);

        try
        {
            var response = await client.TransferAsync(new TransferRequest
            {
                SourceAccountId = sourceAccountId,
                DestinationAccountId = destinationAccountId,
                Amount = amount,
                Description = description
            });

            return response;
        }
        catch (global::Grpc.Core.RpcException ex)
        {
            Console.WriteLine($"Erreur gRPC: {ex.Status.Detail}");
            return new TransactionResponse
            {
                Success = false,
                Message = $"Erreur serveur: {ex.Status.Detail}"
            };
        }
    }

    /// <summary>
    /// Récupère le solde d'un compte spécifique
    /// </summary>
    public static async Task<double?> GetAccountBalanceAsync(int accountId)
    {
        if (Gestion.CurrentUserId is null)
        {
            throw new InvalidOperationException("Aucun utilisateur connecté. Veuillez vous reconnecter.");
        }

        var bankAccounts = await GetBankAccountsAsync();
        var account = bankAccounts.FirstOrDefault(a => a.IdCompte == accountId);
        return account?.Solde;
    }

    /// <summary>
    /// Récupère les détails d'un compte spécifique
    /// </summary>
    public static async Task<Account?> GetAccountByIdAsync(int accountId)
    {
        if (Gestion.CurrentUserId is null)
        {
            throw new InvalidOperationException("Aucun utilisateur connecté. Veuillez vous reconnecter.");
        }

        var bankAccounts = await GetBankAccountsAsync();
        return bankAccounts.FirstOrDefault(a => a.IdCompte == accountId);
    }

    /// <summary>
    /// Valide qu'un compte appartient à l'utilisateur connecté
    /// </summary>
    public static async Task<bool> IsAccountOwnedByUserAsync(int accountId)
    {
        var bankAccounts = await GetBankAccountsAsync();
        return bankAccounts.Any(a => a.IdCompte == accountId);
    }

    /// <summary>
    /// Vérifie si un compte a un solde suffisant pour un montant donné
    /// </summary>
    public static async Task<bool> HasSufficientBalanceAsync(int accountId, double amount)
    {
        var balance = await GetAccountBalanceAsync(accountId);
        return balance.HasValue && balance.Value >= amount;
    }
}
