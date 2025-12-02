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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MiniBanque.Client.View;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginPage : Page
{
    public LoginPage()
    {
        this.InitializeComponent();
    }

    private void OnLoginClicked(object sender, RoutedEventArgs e)
    {
        // TODO: Plus tard, on ajoutera ici la validation gRPC

        // Pour l'instant, on simule une connexion réussie
        // On navigue vers le Dashboard (qu'on va créer juste après)
        // Attention: cette ligne plantera tant que DashboardPage n'existe pas, 
        // donc commente-la si tu n'as pas encore créé la page Dashboard.
       this.Frame.Navigate(typeof(DashboardPage));
    }

    private void OnRegisterClicked(object sender, RoutedEventArgs e)
    {
        // TODO: Naviguer vers la page d'inscription
        this.Frame.Navigate(typeof(RegisterPage));
    }
}
