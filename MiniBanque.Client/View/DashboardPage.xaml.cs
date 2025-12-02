using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
    }
}
