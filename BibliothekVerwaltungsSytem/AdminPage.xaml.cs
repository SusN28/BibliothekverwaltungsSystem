using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BibliothekVerwaltungsSytem;

public partial class AdminPage : Page
{
    public AdminPage()
    {
        InitializeComponent();
    }

    // ── Tabs ───────────────────────────────────────────────
    private void BtnTabUserHinzufuegen_Click(object sender, RoutedEventArgs e)
    {
    }

    private void BtnTabAlleUser_Click(object sender, RoutedEventArgs e)
        => NavigationService?.Navigate(new AlleUserPage());

    // ── User erstellen ─────────────────────────────────────
    private void BtnUserErstellen_Click(object sender, RoutedEventArgs e)
    {
        string vorname  = TxtVorname.Text.Trim();
        string nachname = TxtNachname.Text.Trim();
        string username = TxtUsername.Text.Trim();
        string email    = TxtEmail.Text.Trim();
        string passwort = TxtPasswort.Password;
        string rolle    = (CmbRolle.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "user";

        if (string.IsNullOrEmpty(vorname)  ||
            string.IsNullOrEmpty(nachname) ||
            string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(passwort))
        {
            ZeigeFeedback("Bitte alle Pflichtfelder ausfüllen!", erfolgreich: false);
            return;
        }

        if (passwort.Length < 6)
        {
            ZeigeFeedback("Passwort muss mindestens 6 Zeichen lang sein!", erfolgreich: false);
            return;
        }

        var result = Database.CreateUser(vorname, nachname, username, email, passwort, rolle);

        if (result.Success)
        {
            ZeigeFeedback($" User \"{username}\" erfolgreich erstellt!", erfolgreich: true);
            FormularLeeren();
        }
        else
        {
            ZeigeFeedback($" {result.Error}", erfolgreich: false);
        }
    }

    private void ZeigeFeedback(string nachricht, bool erfolgreich)
    {
        TxtFeedback.Text       = nachricht;
        TxtFeedback.Foreground = erfolgreich
            ? new SolidColorBrush(Color.FromRgb(39, 174, 96))
            : new SolidColorBrush(Color.FromRgb(231, 76, 60));
        TxtFeedback.Visibility = Visibility.Visible;
    }

    private void FormularLeeren()
    {
        TxtVorname.Text        = "";
        TxtNachname.Text       = "";
        TxtUsername.Text       = "";
        TxtEmail.Text          = "";
        TxtPasswort.Password   = "";
        CmbRolle.SelectedIndex = 0;
    }
    
    private void BtnTabBuecher_Click(object sender, RoutedEventArgs e)
        => NavigationService?.Navigate(new BuecherVerwaltenPage());

}
