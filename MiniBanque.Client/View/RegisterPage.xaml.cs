using System;
using System.Text.RegularExpressions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MiniBanque.Grpc;

namespace MiniBanque.Client.View;

public sealed partial class RegisterPage : Page
{
    public RegisterPage()
    {
        this.InitializeComponent();
    }

    private async void OnRegisterSubmitClicked(object sender, RoutedEventArgs e)
    {
        if(string.IsNullOrWhiteSpace(FirstNameBox.Text) ||
            string.IsNullOrWhiteSpace(LastNameBox.Text) ||
            string.IsNullOrWhiteSpace(UsernameBox.Text) ||
            string.IsNullOrWhiteSpace(EmailBox.Text))
        {
            await new ContentDialog
            {
                Title = "Champs requis",
                Content= "Veuillez renseigner tous les champs.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            }.ShowAsync();
            return;
        }

        if(!Regex.IsMatch(EmailBox.Text, @"^\S+@\S+\.\S+$"))
        {
            await new ContentDialog
            {
                Title = "Email invalide",
                Content = "Veuillez saisir un email valide.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            }.ShowAsync();
            return;
        }

        if (PasswordBox.Password != ConfirmPasswordBox.Password || PasswordBox.Password.Length < 6)
        {
            await new ContentDialog
            {
                Title = "Mot de passe invalide",
                Content = "Les mots de passe doivent correspondre et contenir au moins 6 caractères.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            }.ShowAsync();
            return;
        }

        try
        {
            // Activez HTTP/2 clair au démarrage si nécessaire (Program.cs)
            using var channel = GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("MINIBANQUE_API_URL") ?? "http://localhost:5189");
            var client = new BankService.BankServiceClient(channel);

            var request = new RegisterUserRequest
            {
                Username = UsernameBox.Text.Trim(),
                Password = PasswordBox.Password,
                Email = EmailBox.Text.Trim(),
                Prenom = FirstNameBox.Text.Trim(),
                Nom = LastNameBox.Text.Trim()
            };

            var response = await client.RegisterUserAsync(request);

            // Journalisation simple côté client (Output)
            System.Diagnostics.Debug.WriteLine($"[Register] success={response.Success} msg={response.Message} user_id={response.UserId}");

            if (!response.Success)
            {
                await new ContentDialog
                {
                    Title = "Inscription refusée",
                    Content = response.Message ?? "Une erreur est survenue lors de l'inscription.",
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                }.ShowAsync();
                return;
            }

            await new ContentDialog
            {
                Title = "Succès",
                Content = $"Compte créé (ID: {response.UserId}). Message: {response.Message ?? "OK"}",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            }.ShowAsync();

            // Vérification post-inscription: tenter un login (débogage)
            var loginOk = await Controller.Gestion.connect(UsernameBox.Text.Trim(), PasswordBox.Password);
            System.Diagnostics.Debug.WriteLine($"[Register->LoginTest] ok={loginOk}");

            Frame.Navigate(typeof(LoginPage));
        }
        catch (RpcException ex)
        {
            await new ContentDialog
            {
                Title = "Erreur réseau",
                Content = $"{ex.Status.StatusCode}: {ex.Status.Detail}",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            }.ShowAsync();
        }
        catch (Exception ex)
        {
            await new ContentDialog
            {
                Title = "Erreur",
                Content = ex.Message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            }.ShowAsync();
        }
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        this.Frame.Navigate(typeof(LoginPage));
    }
}
