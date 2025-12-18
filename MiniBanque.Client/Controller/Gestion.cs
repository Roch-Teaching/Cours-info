using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using MiniBanque.Grpc;

namespace MiniBanque.Client.Controller;
class Gestion
{
    // Contexte de session utilisateur
    public static int? CurrentUserId { get; private set; }
    public static string? CurrentUsername { get; private set; }

    public static async Task<bool> connect(string name, string pssd)
    {
        //using var channel = GrpcChannel.ForAddress("http://172.20.10.8:5189");

        using var channel = GrpcChannel.ForAddress("http://localhost:5189");
        var client = new BankService.BankServiceClient(channel);

        Console.WriteLine("=== Mini Banque - Client gRPC ===\n");
        Console.WriteLine("1. Test de connexion...");

        var loginResponse = await client.LoginAsync(new LoginRequest
        {
            Username = name,
            Password = pssd
        });

        if (loginResponse.Success)
        {
            CurrentUserId = (int)loginResponse.UserId;
            CurrentUsername = name;

            Console.WriteLine($"✓ Connecté: {loginResponse.Prenom} {loginResponse.Nom} ({loginResponse.Role})");
            Console.WriteLine($"  User ID: {loginResponse.UserId}\n");
            return true;
        }
        else
        {
            Console.WriteLine("✗ Échec de la connexion. Vérifiez vos identifiants.");
            return false;
        }
    }

    // Utilise l’utilisateur connecté, sans demander l’ID
    public static async Task<bool> createAccount(string type, int solde)
    {
        if (CurrentUserId is null)
        {
            throw new InvalidOperationException("Aucun utilisateur connecté. Veuillez vous reconnecter.");
        }

        using var channel = GrpcChannel.ForAddress("http://localhost:5189");
        var client = new BankService.BankServiceClient(channel);

        var _ = await client.CreateAccountAsync(new CreateAccountRequest
        {
            UserId = CurrentUserId.Value,
            Type = type,
            InitialDeposit = solde
        });

        return true;
    }
}
