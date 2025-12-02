using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
// IMPORTANT : On ajoute cette ligne pour que MainPage puisse "voir" ta page de login
using MiniBanque.Client.View;

namespace MiniBanque.Client
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // IMPORTANT : On s'abonne à l'événement "Loaded"
            // C'est ce qui dit : "Dès que la fenêtre est prête, lance la fonction MainPage_Loaded"
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // C'est ici qu'on dit à la Frame d'afficher la page de connexion
            RootFrame.Navigate(typeof(LoginPage));
        }
    }
}
