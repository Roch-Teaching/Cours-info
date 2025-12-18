using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Grpc.Core;

namespace MiniBanque.Client.View;

public sealed partial class CreateAccountPage : Page
{
    public CreateAccountPage()
    {
        InitializeComponent();
    }

    private void OnLogoutClicked(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(LoginPage));
    }

    private void OnSeeAllHistoryClicked(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
        else
        {
            Frame.Navigate(typeof(DashboardPage));
        }
    }

    private async void OnCreateAccountClicked(object sender, RoutedEventArgs e)
    {
        // Réinitialise le message d’erreur
        if (ErrorMesageTextBlock is not null)
        {
            ErrorMesageTextBlock.Visibility = Visibility.Collapsed;
        }

        // Vérifie qu’un utilisateur est connecté
        if (Controller.Gestion.CurrentUserId is null)
        {
            ShowError("Aucun utilisateur connecté. Veuillez vous reconnecter.");
            return;
        }

        // Validation champs
        var selectedItem = AccountTypeComboBox?.SelectedItem as ComboBoxItem;

        var accountTypeKey= (selectedItem?.Tag as string)?.Trim().ToLowerInvariant()
                            ?? selectedItem.Content?.ToString()?.Trim().ToLowerInvariant();

        
        if (string.IsNullOrWhiteSpace(accountTypeKey) || (accountTypeKey != "epargne" && accountTypeKey != "risque" && accountTypeKey != "courant"))
        {
            ShowError("Veuillez sélectionner un type de compte.");
            return;
        }


        var deposit = InitialDepositNumberBox?.Value ?? double.NaN;
        if (double.IsNaN(deposit) || deposit < 0)
        {
            ShowError("Le dépôt initial doit être un montant valide.");
            return;
        }

        if (TermsCheckBox?.IsChecked != true)
        {
            ShowError("Vous devez accepter les conditions générales.");
            return;
        }

        
        CreateButton.IsEnabled = false;
        

        // Appel gRPC via le contrôleur
        try
        {
            var ok = await Controller.Gestion.createAccount(accountTypeKey, (int)System.Math.Round(deposit));
            if (!ok)
            {
                ShowError("La création du compte a échoué.");
                return;
            }

            await new ContentDialog
            {
                Title = "Compte créé",
                Content = $"Le compte « {accountTypeKey} » a été créé avec un dépôt initial de {(int)System.Math.Round(deposit)} €.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            }.ShowAsync();

            Frame.Navigate(typeof(DashboardPage));
        }
        catch (RpcException ex)
        {
            ShowError($"Erreur réseau: {ex.Status.Detail}");
        }
        catch (System.OperationCanceledException)
        {
            ShowError("Délai dépassé. Réessayez");
        }
        catch (System.Exception ex)
        {
            ShowError($"Erreur: {ex.Message}");
        }
        finally
        {
            CreateButton.IsEnabled = true;
        }
        

        void ShowError(string message)
        {
            if (ErrorMesageTextBlock is not null)
            {
                ErrorMesageTextBlock.Text = message;
                ErrorMesageTextBlock.Visibility = Visibility.Visible;
            }
        }
    }
}
