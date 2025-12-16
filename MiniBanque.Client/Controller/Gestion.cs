using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Net.Client;
using MiniBanque.Grpc;
using System;
using System.Threading.Tasks;

namespace MiniBanque.Client.Controller;
 class Gestion
{
    public static async Task connect(string name, string pssd)
    {
        // Créer le canal gRPC
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
        }
        else
        {
            Console.WriteLine("✗ Échec de la connexion. Vérifiez vos identifiants.");
        }
    }
}
