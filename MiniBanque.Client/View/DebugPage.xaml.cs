using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MiniBanque.Client.Controller;
using System.Threading.Tasks;

namespace MiniBanque.Client.View
{
    public sealed partial class DebugPage : Page
    {
        public DebugPage()
        {
            this.InitializeComponent();
            IpAddressText.Text = Gestion.ServerAddress;
            UsernameText.Text = Gestion.CurrentUsername ?? "admin";
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginPage));
        }

        private async void OnTestClicked(object sender, RoutedEventArgs e)
        {
            string ip = IpAddressText.Text;
            string user = UsernameText.Text;
            string pass = PasswordText.Text;
            string cmd = CommandText.Text?.ToLower().Trim();

            DebugOutputText.Text = "⏳ Running test...";

            try
            {
                Gestion.ServerAddress = ip;

                if (cmd == "ping")
                {
                    DebugOutputText.Text = $"Status: Online\nSession ID: {Gestion.CurrentUserId ?? -1}";
                    return;
                }

                if (cmd == "clear")
                {
                    DebugOutputText.Text = "Session cleared.";
                    return;
                }

                bool success = await Gestion.connect(user, pass);

                if (success)
                {
                    DebugOutputText.Text = $"✅ SUCCESS\nUser ID: {Gestion.CurrentUserId}\nConnected to: {ip}";
                }
                else
                {
                    DebugOutputText.Text = "❌ FAILURE: Invalid credentials or Server unreachable.";
                }
            }
            catch (Exception ex)
            {
                DebugOutputText.Text = $"⚠️ ERROR: {ex.Message}";
            }
        }
        private void OnUpdateAddressClicked(object sender, RoutedEventArgs e)
        {
            string newAddress = IpAddressText.Text?.Trim();

            if (string.IsNullOrEmpty(newAddress))
            {
                DebugOutputText.Text = "⚠️ L'adresse ne peut pas être vide.";
                return;
            }

            try
            {
                Gestion.ServerAddress = newAddress;
                DebugOutputText.Text = $"✅ Adresse mise à jour : {Gestion.ServerAddress}";
            }
            catch (Exception ex)
            {
                DebugOutputText.Text = $"❌ Erreur : {ex.Message}";
            }
        }
    }
}
