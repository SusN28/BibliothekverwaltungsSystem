using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BibliothekVerwaltungsSytem;

public partial class BuecherPage : Page
{
    private ObservableCollection<Database.BuchInfo> _alleBuecher = new();
    private bool _hatUeberfaellige = false;

    public BuecherPage()
    {
        InitializeComponent();
        PruefeUeberfaellige();
        LadeBuecher();
    }

    private void PruefeUeberfaellige()
    {
        var ausleihen = Database.LoadAusleihenVonUser(Session.CurrentUser!.UserId);
        _hatUeberfaellige = ausleihen.Any(a => a.IstUeberfaellig);

        if (_hatUeberfaellige)
            PanelGesperrt.Visibility = Visibility.Visible;
    }

    private void LadeBuecher()
    {
        _alleBuecher = Database.LoadVerfuegbareBuecher();
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
                b.Titel.ToLower().Contains(suche)     ||
                b.AutorName.ToLower().Contains(suche) ||
                b.Isbn.ToLower().Contains(suche)));
    }

    // ── Ausleihen ──────────────────────────────────────────
    private void BtnAusleihen_Click(object sender, RoutedEventArgs e)
    {
        // Gesperrt wenn ueberfaellige Buecher vorhanden
        if (_hatUeberfaellige)
        {
            MessageBox.Show(
                "Du hast noch überfällige Bücher.\nBitte gib diese zuerst zurück.",
                "Ausleihen gesperrt",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        int buchId = (int)((Button)sender).Tag;
        var buch = _alleBuecher.FirstOrDefault(b => b.BuchId == buchId);
        if (buch == null) return;

        var fenster = new AusleihenWindow(buch);
        fenster.ShowDialog();

        if (fenster.Ausgeliehen)
            LadeBuecher();
    }

    // ── Tab-Navigation ─────────────────────────────────────
    private void BtnInventar_Click(object sender, RoutedEventArgs e)
        => NavigationService?.Navigate(new InventarPage());

    private void BtnSuche_Click(object sender, RoutedEventArgs e)
        => NavigationService?.Navigate(new SuchePage());

    private void BtnBuecher_Click(object sender, RoutedEventArgs e)
    {
        // Schon auf der Seite
    }
}
