using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MiniBanque.Client.View
{
    public sealed partial class HistoryPage : Page
    {
        public HistoryPage()
        {
            this.InitializeComponent();
        }

        private void OnBackClicked(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
            else
                this.Frame.Navigate(typeof(DashboardPage));
        }
    }
}
