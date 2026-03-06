using System.Windows;

namespace BibliothekVerwaltungsSytem;

public partial class AutorHinzufuegenWindow : Window
{
    public AutorHinzufuegenWindow()
    {
        InitializeComponent();
    }

    private void BtnSpeichern_Click(object sender, RoutedEventArgs e)
    {
        string vorname      = TxtVorname.Text.Trim();
        string nachname     = TxtNachname.Text.Trim();
        string geburtsjahr  = TxtGeburtsjahr.Text.Trim();
        string nationalitaet = TxtNationalitaet.Text.Trim();
        string biografie    = TxtBiografie.Text.Trim();

        if (string.IsNullOrEmpty(nachname))
        {
            MessageBox.Show("Nachname darf nicht leer sein.", "Hinweis",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Geburtsjahr validieren falls angegeben
        if (!string.IsNullOrEmpty(geburtsjahr) &&
            (!int.TryParse(geburtsjahr, out int jahr) || jahr < 1000 || jahr > 2100))
        {
            MessageBox.Show("Bitte ein gueltiges Geburtsjahr eingeben (z.B. 1965).", "Hinweis",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = Database.CreateAutor(vorname, nachname, geburtsjahr, nationalitaet, biografie);

        if (result.Success)
        {
            MessageBox.Show($"Autor \"{vorname} {nachname}\" wurde erfolgreich gespeichert.",
                "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
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