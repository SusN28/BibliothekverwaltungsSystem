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
        btnLogin.Click += BtnLogin_Click;
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        string username = txtUsername.Text;
        string password = txtPassword.Password;

        if (username == "user" && password == "user")
        {
            NavigateToUserPage();
        }
        else if (username == "admin" && password == "admin")
        {
            NavigateToAdminPage();
        }
        else
        {
            MessageBox.Show("Falsche Anmeldedaten!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void NavigateToUserPage()
    {
        loginPanel.Visibility = Visibility.Collapsed;
        mainFrame.Visibility = Visibility.Visible;
        mainFrame.Navigate(new UserPage());
        WindowState = WindowState.Maximized; // vollbild
    }

    private void NavigateToAdminPage()
    {
        loginPanel.Visibility = Visibility.Collapsed;
        mainFrame.Visibility = Visibility.Visible;
        mainFrame.Navigate(new AdminPage());
        WindowState = WindowState.Maximized;
    }
}