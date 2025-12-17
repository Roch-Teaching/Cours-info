using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MiniBanque.Client.View;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginPage : Page
{
    private bool isConnected = false;
    public LoginPage()
    {
        this.InitializeComponent();
    }

    private async void OnLoginClicked(object sender, RoutedEventArgs e)
    {
        string username = usernameTextBox.Text;
        string password = PasswordBox.Password;
        if (username != null && password != null)
        {
            isConnected = await Controller.Gestion.connect(username, password);
            if (isConnected)
            {
                this.Frame.Navigate(typeof(DashboardPage));
            }
            else
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Erreur de connexion",
                    Content = "Échec de la connexion. Vérifiez vos identifiants.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }
    }

    private void OnRegisterClicked(object sender, RoutedEventArgs e)
    {
        // TODO: Naviguer vers la page d'inscription
        this.Frame.Navigate(typeof(RegisterPage));
    }

    private void OnDebugClicked(object sender, RoutedEventArgs e)
    {
        this.Frame.Navigate(typeof(DebugPage));
    }
}
