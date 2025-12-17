using System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grpc.Net.Client;
using MiniBanque.Grpc;
using Windows.Media.Protection.PlayReady;
using Windows.System;

namespace MiniBanque.Client.Controller;
 class Gestion

{

    public static async Task<bool> connect(string name, string pssd)
    {
        using var channel = GrpcChannel.ForAddress("http://10.93.25.39:5189");
        var client = new BankService.BankServiceClient(channel);


        Console.WriteLine("=== Mini Banque - Client gRPC ===\n");

        // 1. Connexion
        Console.WriteLine("1. Test de connexion...");
        var loginResponse = await client.LoginAsync(new LoginRequest
        {
            Username = name,
            Password = pssd
        });

        if (loginResponse.Success)
        {
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
    public async void createAccount(int userId,string type,int solde)
    {
        using var channel = GrpcChannel.ForAddress("http://10.93.25.39:5189");
        var client = new BankService.BankServiceClient(channel);
        Console.WriteLine("2. Création d'un compte épargne...");
        var createAccountResponse = await client.CreateAccountAsync(new CreateAccountRequest
        {
            UserId = userId,
            Type = type,
            InitialDeposit = solde
        });
        
    }
}
