using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BibliothekVerwaltungsSytem;

public partial class UserPage : Page
{
    public UserPage()
    {
        InitializeComponent();

        if (Session.CurrentUser != null)
        {
            TxtWillkommen.Text = $"Willkommen, {Session.CurrentUser.Vorname} {Session.CurrentUser.Nachname}!";
            PruefeUeberfaelligeBuecher();
        }
    }

    private void PruefeUeberfaelligeBuecher()
    {
        var alleAusleihen = Database.LoadAusleihenVonUser(Session.CurrentUser!.UserId);

        var ueberfaellig = alleAusleihen
            .Where(a => a.IstUeberfaellig)
            .ToList();

        if (ueberfaellig.Count == 0) return;

        // Warnung anzeigen
        ListeUeberfaellig.ItemsSource = ueberfaellig;
        PanelUeberfaellig.Visibility  = Visibility.Visible;
    }

    private void WeiterButton_Click(object sender, RoutedEventArgs e)
        => NavigationService?.Navigate(new InventarPage());
}