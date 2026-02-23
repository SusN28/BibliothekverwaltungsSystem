using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BibliothekVerwaltungsSytem;

public partial class AlleUserPage : Page
{
    private ObservableCollection<Database.UserInfo> _alleUser = new();

    public AlleUserPage()
    {
        InitializeComponent();
        LadeAlleUser();
    }

    // ── Laden ──────────────────────────────────────────────
    private void LadeAlleUser()
    {
        _alleUser = Database.LoadAlleUser();
        UserDataGrid.ItemsSource = _alleUser;
    }

    // ── Suche ──────────────────────────────────────────────
    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string suche = SearchTextBox.Text.Trim().ToLower();

        if (string.IsNullOrEmpty(suche))
        {
            UserDataGrid.ItemsSource = _alleUser;
            return;
        }

        UserDataGrid.ItemsSource = new ObservableCollection<Database.UserInfo>(
            _alleUser.Where(u =>
                u.Username.ToLower().Contains(suche) ||
                u.Vorname.ToLower().Contains(suche)  ||
                u.Nachname.ToLower().Contains(suche) ||
                u.Email.ToLower().Contains(suche)));
    }

    // ── User löschen ───────────────────────────────────────
    private void BtnUserLoeschen_Click(object sender, RoutedEventArgs e)
    {
        int userId = (int)((Button)sender).Tag;

        var user = _alleUser.FirstOrDefault(u => u.UserId == userId);
        if (user == null) return;

        // Eigenen Account schützen
        if (userId == Session.CurrentUser!.UserId)
        {
            MessageBox.Show("Du kannst deinen eigenen Account nicht löschen!",
                "Nicht erlaubt", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Bestätigungsdialog
        var bestaetigung = MessageBox.Show(
            $"User \"{user.Username}\" ({user.Vorname} {user.Nachname}) wirklich löschen?\n\nAlle Ausleihen dieses Users werden ebenfalls gelöscht!",
            "User löschen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (bestaetigung != MessageBoxResult.Yes) return;

        var result = Database.DeleteUser(userId);

        if (result.Success)
        {
            _alleUser.Remove(user);
            MessageBox.Show($" User \"{user.Username}\" wurde erfolgreich gelöscht.",
                "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show($" {result.Error}", "Fehler",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ── Tab-Navigation ─────────────────────────────────────
    private void BtnTabUserHinzufuegen_Click(object sender, RoutedEventArgs e)
        => NavigationService?.Navigate(new AdminPage());

    private void BtnTabAlleUser_Click(object sender, RoutedEventArgs e)
    {
        // Schon auf der Seite
    }
}
