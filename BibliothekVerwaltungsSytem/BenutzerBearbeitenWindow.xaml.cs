using System.Windows;
using System.Windows.Controls;

namespace BibliothekVerwaltungsSytem;

public partial class BenutzerBearbeitenWindow : Window
{
    private readonly int _userId;

    // Gibt zurück ob gespeichert wurde (damit AlleUserPage neu laden kann)
    public bool Gespeichert { get; private set; } = false;

    public BenutzerBearbeitenWindow(Database.UserInfo user)
    {
        InitializeComponent();

        _userId = user.UserId;

        // Felder vorausfüllen
        TxtSubtitel.Text  = $"ID: {user.UserId}  |  Erstellt als: {user.Username}";
        TxtVorname.Text   = user.Vorname;
        TxtNachname.Text  = user.Nachname;
        TxtUsername.Text  = user.Username;
        TxtEmail.Text     = user.Email;

        // Rolle vorauswählen
        CmbRolle.SelectedIndex = user.Rolle == "admin" ? 1 : 0;
    }

    private void BtnSpeichern_Click(object sender, RoutedEventArgs e)
    {
        string vorname  = TxtVorname.Text.Trim();
        string nachname = TxtNachname.Text.Trim();
        string username = TxtUsername.Text.Trim();
        string email    = TxtEmail.Text.Trim();
        string passwort = TxtPasswort.Password; // leer = nicht ändern
        string rolle    = (CmbRolle.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "user";

        if (string.IsNullOrEmpty(vorname)  ||
            string.IsNullOrEmpty(nachname) ||
            string.IsNullOrEmpty(username))
        {
            MessageBox.Show("Vorname, Nachname und Benutzername duerfen nicht leer sein.",
                "Hinweis", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!string.IsNullOrEmpty(passwort) && passwort.Length < 6)
        {
            MessageBox.Show("Das neue Passwort muss mindestens 6 Zeichen lang sein.",
                "Hinweis", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = Database.UpdateUser(_userId, vorname, nachname, username, email, passwort, rolle);

        if (result.Success)
        {
            Gespeichert = true;
            Close();
        }
        else
        {
            MessageBox.Show($"Fehler: {result.Error}", "Fehler",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnAbbrechen_Click(object sender, RoutedEventArgs e)
        => Close();
}
