using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MiniBanque.Client.Controller;
using MiniBanque.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniBanque.Client.View
{
    public sealed partial class TransferPage : Page
    {
        private List<Account> bankAccounts = new(); // initialise pour éviter CS8618

        public TransferPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await LoadBankAccountsAsync();
        }

        private async System.Threading.Tasks.Task LoadBankAccountsAsync()
        {
            try
            {
                LoadingRing.IsActive = true;
                ValidateButton.IsEnabled = false;

                // Récupération des comptes bancaires de l'utilisateur
                bankAccounts = await TransferController.GetBankAccountsAsync();

                if (bankAccounts != null && bankAccounts.Any())
                {
                    var displayItems = bankAccounts.Select(a => new
                    {
                        Account = a,
                        DisplayText = $"{a.Type} - ID: {a.IdCompte} ({a.Solde:F2} €)"
                    }).ToList();

                    SourceAccountCombo.ItemsSource = displayItems;
                    SourceAccountCombo.DisplayMemberPath = "DisplayText";
                    SourceAccountCombo.SelectedIndex = 0;
                }
                else
                {
                    ShowMessage("Aucun compte trouvé", InfoBarSeverity.Warning);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur lors du chargement des comptes : {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                LoadingRing.IsActive = false;
                ValidateButton.IsEnabled = true;
            }
        }

        // Ajouté pour corriger CS1061 : handler utilisé dans le XAML
        private void SourceAccountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Pour l’instant, aucun traitement spécifique
            // Vous pourrez y ajouter de la logique plus tard si nécessaire
        }

        private async void OnTransferClicked(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            try
            {
                LoadingRing.IsActive = true;
                ValidateButton.IsEnabled = false;

                dynamic selectedSourceItem = SourceAccountCombo.SelectedItem;
                Account sourceAccount = selectedSourceItem.Account;

                if (!int.TryParse(DestinationAccountIdBox.Text.Trim(), out int destinationAccountId))
                {
                    ShowMessage("L'ID du compte destinataire doit être un nombre entier valide", InfoBarSeverity.Warning);
                    return;
                }

                bool isOwned = await TransferController.IsAccountOwnedByUserAsync(sourceAccount.IdCompte);
                if (!isOwned)
                {
                    ShowMessage("Le compte source sélectionné n'appartient pas à l'utilisateur connecté", InfoBarSeverity.Error);
                    return;
                }

                bool hasSufficientBalance = await TransferController.HasSufficientBalanceAsync(
                    sourceAccount.IdCompte,
                    AmountBox.Value
                );

                if (!hasSufficientBalance)
                {
                    ShowMessage("Solde insuffisant pour effectuer ce virement", InfoBarSeverity.Error);
                    return;
                }

                var response = await TransferController.MakeTransferAsync(
                    sourceAccountId: sourceAccount.IdCompte,
                    destinationAccountId: destinationAccountId,
                    amount: AmountBox.Value,
                    description: DescriptionBox.Text.Trim()
                );

                if (response.Success)
                {
                    ShowMessage(
                        $"Virement effectué avec succès ! Transaction ID: {response.TransactionId}. Nouveau solde: {response.NewBalance:F2} €",
                        InfoBarSeverity.Success
                    );

                    await System.Threading.Tasks.Task.Delay(2000);
                    this.Frame.Navigate(typeof(DashboardPage));
                }
                else
                {
                    ShowMessage($"Échec du virement : {response.Message}", InfoBarSeverity.Error);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur : {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                LoadingRing.IsActive = false;
                ValidateButton.IsEnabled = true;
            }
        }

        private bool ValidateInputs()
        {
            if (SourceAccountCombo.SelectedItem == null)
            {
                ShowMessage("Veuillez sélectionner un compte à débiter", InfoBarSeverity.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(DestinationAccountIdBox.Text))
            {
                ShowMessage("Veuillez saisir l'ID du compte destinataire", InfoBarSeverity.Warning);
                return false;
            }

            if (AmountBox.Value <= 0)
            {
                ShowMessage("Le montant doit être supérieur à 0", InfoBarSeverity.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(DescriptionBox.Text))
            {
                ShowMessage("Veuillez entrer un motif de virement", InfoBarSeverity.Warning);
                return false;
            }

            return true;
        }

        private void ShowMessage(string message, InfoBarSeverity severity)
        {
            MessageInfoBar.Message = message;
            MessageInfoBar.Severity = severity;
            MessageInfoBar.IsOpen = true;
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}
