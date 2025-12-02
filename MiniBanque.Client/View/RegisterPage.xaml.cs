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
public sealed partial class RegisterPage : Page
{
    public RegisterPage()
    {
        this.InitializeComponent();
    }

    private void OnRegisterSubmitClicked(object sender, RoutedEventArgs e)
    {
        // TODO: Plus tard, vérifier que les mots de passe sont identiques
        // TODO: Envoyer les données (Prénom, Nom, Email...) au serveur gRPC

        // Pour l'instant, on simule une réussite et on retourne au login
        this.Frame.Navigate(typeof(LoginPage));

    }

    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        // Retour à la page de connexion si on annule
        this.Frame.Navigate(typeof(LoginPage));
    }


}
