using MiniBanque.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration de la connexion à la base de données
// IMPORTANT: Modifiez cette chaîne selon votre configuration MySQL
var connectionString = "Server=127.0.0.1;Database=mini_banque;Uid=root;Pwd=;";

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

app.Run();