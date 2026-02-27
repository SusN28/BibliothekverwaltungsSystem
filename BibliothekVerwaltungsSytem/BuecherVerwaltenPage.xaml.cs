using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BibliothekVerwaltungsSytem;

public partial class BuecherVerwaltenPage : Page
{
    private ObservableCollection<Database.BuchInfo> _alleBuecher = new();

    public BuecherVerwaltenPage()
    {
        InitializeComponent();
        LadeBuecher();
    }

    private void LadeBuecher()
    {
        _alleBuecher = Database.LoadAlleBuecher();
        BuecherDataGrid.ItemsSource = _alleBuecher;
    }

    // ── Suche ──────────────────────────────────────────────
    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string suche = SearchTextBox.Text.Trim().ToLower();

        if (string.IsNullOrEmpty(suche))
        {
            BuecherDataGrid.ItemsSource = _alleBuecher;
            return;
        }

        BuecherDataGrid.ItemsSource = new ObservableCollection<Database.BuchInfo>(
            _alleBuecher.Where(b =>
                b.Titel.ToLower().Contains(suche)      ||
                b.AutorName.ToLower().Contains(suche)  ||
                b.Isbn.ToLower().Contains(suche)       ||
                b.Verlag.ToLower().Contains(suche)));
    }

    // ── Buch hinzufuegen ───────────────────────────────────
    private void BtnBuchHinzufuegen_Click(object sender, RoutedEventArgs e)
    {
        var fenster = new BuchBearbeitenWindow(null); // null = Hinzufuegen-Modus
        fenster.ShowDialog();
        if (fenster.Gespeichert)
            LadeBuecher();
    }

    // ── Buch bearbeiten ────────────────────────────────────
    private void BtnBuchBearbeiten_Click(object sender, RoutedEventArgs e)
    {
        int buchId = (int)((Button)sender).Tag;
        var buch = _alleBuecher.FirstOrDefault(b => b.BuchId == buchId);
        if (buch == null) return;

        var fenster = new BuchBearbeitenWindow(buch);
        fenster.ShowDialog();
        if (fenster.Gespeichert)
            LadeBuecher();
    }

    // ── Buch loeschen ──────────────────────────────────────
    private void BtnBuchLoeschen_Click(object sender, RoutedEventArgs e)
    {
        int buchId = (int)((Button)sender).Tag;
        var buch = _alleBuecher.FirstOrDefault(b => b.BuchId == buchId);
        if (buch == null) return;

        var bestaetigung = MessageBox.Show(
            $"Buch \"{buch.Titel}\" wirklich loeschen?\n\nAlle Ausleihen zu diesem Buch werden ebenfalls geloescht!",
            "Buch loeschen", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (bestaetigung != MessageBoxResult.Yes) return;

        var result = Database.DeleteBuch(buchId);
        if (result.Success)
        {
            _alleBuecher.Remove(buch);
            MessageBox.Show($"Buch \"{buch.Titel}\" wurde geloescht.",
                "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show($"Fehler: {result.Error}", "Fehler",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ── Tab-Navigation ─────────────────────────────────────
    private void BtnTabUserHinzufuegen_Click(object sender, RoutedEventArgs e)
        => NavigationService?.Navigate(new AdminPage());

    private void BtnTabAlleUser_Click(object sender, RoutedEventArgs e)
        => NavigationService?.Navigate(new AlleUserPage());

    private void BtnTabBuecher_Click(object sender, RoutedEventArgs e)
    {
        // Schon auf der Seite
    }
}
