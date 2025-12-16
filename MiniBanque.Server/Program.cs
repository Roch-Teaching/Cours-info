using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using MiniBanque.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration de la connexion à la base de données
// IMPORTANT: Modifiez cette chaîne selon votre configuration MySQL
var connectionString = "Server=127.0.0.1;Database=mini_banque;Uid=root;Pwd=;";

// Configurer Kestrel pour écouter toutes les interfaces réseau
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP/2 en clair (pour tests). Écoute sur 0.0.0.0:5189 (toutes les interfaces)
    options.Listen(IPAddress.Any, 5189, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });

    // HTTPS (HTTP/2) si vous disposez d'un certificat. Décommentez et configurez pour la prod.
    // options.Listen(IPAddress.Any, 7168, listenOptions =>
    // {
    //     listenOptions.UseHttps("chemin/vers/votreCertificat.pfx", "motDePasseDuPfx");
    //     listenOptions.Protocols = HttpProtocols.Http2;
    // });
});

// Ajouter les services gRPC
builder.Services.AddGrpc();

// Ajouter le service de banque avec la chaîne de connexion
builder.Services.AddSingleton(new BankServiceImpl(connectionString));

var app = builder.Build();

// Configurer le pipeline HTTP
app.MapGrpcService<BankServiceImpl>();

// Page d'accueil pour informer que c'est un service gRPC
app.MapGet("/", () => "Ce service utilise gRPC. " +
                      "Pour communiquer avec ce service, vous devez utiliser un client gRPC. " +
                      "Consultez https://go.microsoft.com/fwlink/?linkid=2086909 pour plus d'informations.");

Console.WriteLine("Serveur gRPC de la Mini Banque démarré...");
Console.WriteLine($"Connexion à la base de données: mini_banque");
Console.WriteLine("Kestrel configuré pour écouter toutes les interfaces (0.0.0.0) sur le port 5189 (HTTP/2)");
// Si HTTPS activé, indiquer aussi le port HTTPS
// Console.WriteLine("Kestrel configuré pour HTTPS sur le port 7168 (HTTP/2)");

app.Run();