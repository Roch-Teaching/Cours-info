using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MiniBanque.Client.View
{
    public sealed partial class TransferPage : Page
    {
        public TransferPage()
        {
            this.InitializeComponent();
        }

        private void OnTransferClicked(object sender, RoutedEventArgs e)
        {
            // TODO: Vérifier le solde et envoyer la requête gRPC au serveur

            // Pour l'instant, on simule une réussite et on retourne au dashboard
            // Idéalement, on afficherait un petit message "Succès" avant
            this.Frame.Navigate(typeof(DashboardPage));
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            // Retour au tableau de bord sans rien faire
            this.Frame.GoBack();
        }
    }
}
