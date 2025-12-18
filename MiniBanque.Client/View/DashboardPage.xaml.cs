using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Threading.Tasks;
using MiniBanque.Grpc;
using Grpc.Net.Client;

namespace MiniBanque.Client.View
{
    public sealed partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            this.InitializeComponent();
        }

        private void OnLogoutClicked(object sender, RoutedEventArgs e)
        {
            // Retour Login
            this.Frame.Navigate(typeof(LoginPage));
        }

        private void OnNewTransferClicked(object sender, RoutedEventArgs e)
        {
            // Navigation vers la page Virement
            this.Frame.Navigate(typeof(TransferPage));
        }
        private void OnSeeAllHistoryClicked(object sender, RoutedEventArgs e)
        {
            // Navigation vers la page d'historique complet
            this.Frame.Navigate(typeof(HistoryPage));
        }

        private void OnCreateAccountClicked(object sender, RoutedEventArgs e)
        {
            // Navigation vers la page de création de compte
            this.Frame.Navigate(typeof(CreateAccountPage));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUserDataAsync();
        }

        private async Task LoadUserDataAsync()
        {
            if (Controller.Gestion.CurrentUserId is null)
            {
                if (GreetingTextBlock != null)
                    GreetingTextBlock.Text = "Bonjour, Utilisateur";
                return;
            }

            if (GreetingTextBlock != null)
                GreetingTextBlock.Text = $"Bonjour, {Controller.Gestion.CurrentUsername ?? "Utilisateur"}";

            try
            {
                using var channel = GrpcChannel.ForAddress(Controller.Gestion.ServerAddress);
                var client = new BankService.BankServiceClient(channel);

                var resp = await client.GetUserAccountsAsync(new GetUserAccountsRequest { UserId = Controller.Gestion.CurrentUserId.Value });
                if (resp.Success && AccountsListView != null)
                {
                    AccountsListView.Items.Clear();
                    double total = 0;
                    foreach (var acc in resp.Accounts)
                    {
                        total += acc.Solde;

                        var panel = new StackPanel { Orientation = Microsoft.UI.Xaml.Controls.Orientation.Horizontal, Spacing = 16, Padding = new Thickness(12) };

                        var icon = new FontIcon { FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"), FontSize = 24 };
                        icon.Glyph = acc.Type?.ToLowerInvariant() switch
                        {
                            "courant" => "\uE825",
                            "epargne" => "\uE7C1",
                            _ => "\uE8C7"
                        };

                        var col = new StackPanel { Width = 150 };
                        col.Children.Add(new TextBlock { Text = acc.Type ?? "Compte", FontWeight = Microsoft.UI.Text.FontWeights.Bold });
                        col.Children.Add(new TextBlock { Text = $"**** {acc.IdCompte}", FontSize = 12, Opacity = 0.5 });

                        var bal = new TextBlock { Text = FormatCurrency(acc.Solde), FontWeight = Microsoft.UI.Text.FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center };

                        panel.Children.Add(icon);
                        panel.Children.Add(col);
                        panel.Children.Add(bal);

                        AccountsListView.Items.Add(new ListViewItem { Content = panel });
                    }

                    if (TotalBalanceTextBlock != null)
                        TotalBalanceTextBlock.Text = FormatCurrency(total);
                }

                if (TransactionsListView != null)
                    TransactionsListView.Items.Clear();

                if (resp.Success && resp.Accounts.Count > 0)
                {
                    var firstId = resp.Accounts[0].IdCompte;
                    var txResp = await client.GetTransactionsAsync(new GetTransactionsRequest { AccountId = firstId, Limit = 5 });
                    if (txResp.Success)
                    {
                        int ops = 0; double entries = 0, exits = 0;
                        foreach (var tx in txResp.Transactions)
                        {
                            ops++;
                            var grid = new Grid { Padding = new Thickness(12, 8, 12, 8) };
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                            var left = new StackPanel { Width = 60 };
                            left.Children.Add(new TextBlock { Text = (tx.DateTransaction?.Split(' ').Length > 0 ? tx.DateTransaction.Split(' ')[0] : ""), FontWeight = Microsoft.UI.Text.FontWeights.Bold });
                            left.Children.Add(new TextBlock { Text = (tx.DateTransaction?.Split(' ').Length > 1 ? tx.DateTransaction.Split(' ')[1] : ""), FontSize = 11, Opacity = 0.5 });

                            var center = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) };
                            center.Children.Add(new TextBlock { Text = tx.Description ?? tx.TypeTransaction, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
                            center.Children.Add(new TextBlock { Text = tx.TypeTransaction ?? "", FontSize = 12, Opacity = 0.6 });

                            var amountText = tx.Montant >= 0 ? $"+ {FormatCurrency(tx.Montant)}" : $"- {FormatCurrency(Math.Abs(tx.Montant))}";
                            var amountBlock = new TextBlock { Text = amountText, Foreground = tx.Montant >= 0 ? new SolidColorBrush(Microsoft.UI.Colors.Green) : new SolidColorBrush(Microsoft.UI.Colors.Red), FontWeight = Microsoft.UI.Text.FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center };

                            Grid.SetColumn(left, 0);
                            Grid.SetColumn(center, 1);
                            Grid.SetColumn(amountBlock, 2);

                            grid.Children.Add(left);
                            grid.Children.Add(center);
                            grid.Children.Add(amountBlock);

                            if (TransactionsListView != null)
                                TransactionsListView.Items.Add(new ListViewItem { Content = grid });

                            if (tx.Montant >= 0) entries += tx.Montant; else exits += Math.Abs(tx.Montant);
                        }

                        if (OpsTextBlock != null)
                            OpsTextBlock.Text = $"{ops} Ops";
                        if (EntriesTextBlock != null)
                            EntriesTextBlock.Text = $"+ {FormatCurrency(entries)}";
                        if (ExitsTextBlock != null)
                            ExitsTextBlock.Text = $"- {FormatCurrency(exits)}";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load dashboard: " + ex.Message);
            }
        }

        private string FormatCurrency(double amount)
        {
            return string.Format(System.Globalization.CultureInfo.GetCultureInfo("fr-FR"), "{0:N2} €", amount);
        }
    }
}
