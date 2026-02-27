using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using BibliothekVerwaltungsSytem;

namespace BibliothekVerwaltungsSytem;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // EINMALIG: Plaintext-Passwörter in der DB auf BCrypt migrieren.
        // Nach dem ersten erfolgreichen Start diese Zeile wieder entfernen!
       // Database.MigratePasswordsToBCrypt();

        btnLogin.Click += BtnLogin_Click;
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Password;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Bitte Benutzername und Passwort eingeben.", "Hinweis",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = Database.LoginUser(username, password);

        if (!result.Success)
        {
            MessageBox.Show(result.Error, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Eingeloggten User global speichern
        Session.CurrentUser = result.User;

        if (result.User!.Rolle == "admin")
            NavigateToAdminPage();
        else
            NavigateToUserPage();
    }

    private void NavigateToUserPage()
    {
        loginPanel.Visibility = Visibility.Collapsed;
        mainFrame.Visibility  = Visibility.Visible;
        mainFrame.Navigate(new UserPage());
        WindowState = WindowState.Maximized;
    }

    private void NavigateToAdminPage()
    {
        loginPanel.Visibility = Visibility.Collapsed;
        mainFrame.Visibility  = Visibility.Visible;
        mainFrame.Navigate(new AdminPage());
        WindowState = WindowState.Maximized;
    }
}