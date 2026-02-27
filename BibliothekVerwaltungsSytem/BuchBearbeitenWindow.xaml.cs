using System.Windows;
using System.Windows.Controls;

namespace BibliothekVerwaltungsSytem;

public partial class BuchBearbeitenWindow : Window
{
    private readonly int? _buchId; // null = Hinzufuegen-Modus
    public bool Gespeichert { get; private set; } = false;

    public BuchBearbeitenWindow(Database.BuchInfo? buch)
    {
        InitializeComponent();

        // Autoren und Kategorien laden
        CmbAutor.ItemsSource    = Database.LoadAlleAutoren();
        CmbKategorie.ItemsSource = Database.LoadAlleKategorien();

        if (buch == null)
        {
            // Hinzufuegen-Modus
            _buchId              = null;
            TxtTitel.Text        = "Neues Buch hinzufuegen";
            TxtSubtitel.Text     = "Alle Pflichtfelder ausfullen";
            TxtSprache.Text      = "Deutsch";
            TxtAnzahlExemplare.Text = "1";
        }
        else
        {
            // Bearbeiten-Modus
            _buchId = buch.BuchId;
            TxtTitel.Text    = "Buch bearbeiten";
            TxtSubtitel.Text = $"ID: {buch.BuchId}  |  {buch.Titel}";

            TxtBuchTitel.Text        = buch.Titel;
            TxtIsbn.Text             = buch.Isbn;
            TxtErscheinungsjahr.Text = buch.Erscheinungsjahr;
            TxtVerlag.Text           = buch.Verlag;
            TxtSeitenzahl.Text       = buch.Seitenzahl;
            TxtSprache.Text          = buch.Sprache;
            TxtAnzahlExemplare.Text  = buch.AnzahlExemplare.ToString();
            TxtBeschreibung.Text     = buch.Beschreibung;

            // Autor vorauswaehlen
            foreach (Database.AutorInfo item in CmbAutor.Items)
                if (item.AutorId == buch.AutorId)
                {
                    CmbAutor.SelectedItem = item;
                    break;
                }

            // Kategorie vorauswaehlen
            foreach (Database.KategorieInfo item in CmbKategorie.Items)
                if (item.KategorieId == buch.KategorieId)
                {
                    CmbKategorie.SelectedItem = item;
                    break;
                }
        }
    }

    private void BtnSpeichern_Click(object sender, RoutedEventArgs e)
    {
        string titel  = TxtBuchTitel.Text.Trim();
        string isbn   = TxtIsbn.Text.Trim();
        string jahr   = TxtErscheinungsjahr.Text.Trim();
        string verlag = TxtVerlag.Text.Trim();
        string seiten = TxtSeitenzahl.Text.Trim();
        string sprache = TxtSprache.Text.Trim();
        string beschreibung = TxtBeschreibung.Text.Trim();

        if (string.IsNullOrEmpty(titel))
        {
            MessageBox.Show("Titel darf nicht leer sein.", "Hinweis",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CmbAutor.SelectedItem is not Database.AutorInfo autor)
        {
            MessageBox.Show("Bitte einen Autor auswaehlen.", "Hinweis",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(TxtAnzahlExemplare.Text.Trim(), out int exemplare) || exemplare < 1)
        {
            MessageBox.Show("Anzahl Exemplare muss eine positive Zahl sein.", "Hinweis",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int? kategorieId = (CmbKategorie.SelectedItem as Database.KategorieInfo)?.KategorieId;

        LoginResult result;

        if (_buchId == null)
        {
            // Neu erstellen
            result = Database.CreateBuch(titel, isbn, autor.AutorId, kategorieId,
                jahr, verlag, seiten, sprache, exemplare, beschreibung);
        }
        else
        {
            // Bearbeiten
            result = Database.UpdateBuch(_buchId.Value, titel, isbn, autor.AutorId, kategorieId,
                jahr, verlag, seiten, sprache, exemplare, beschreibung);
        }

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
