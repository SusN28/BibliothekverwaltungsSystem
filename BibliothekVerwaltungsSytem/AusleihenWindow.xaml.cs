using System;
using System.Windows;

namespace BibliothekVerwaltungsSytem;

public partial class AusleihenWindow : Window
{
    private readonly Database.BuchInfo _buch;
    public bool Ausgeliehen { get; private set; } = false;

    public AusleihenWindow(Database.BuchInfo buch)
    {
        InitializeComponent();
        _buch = buch;

        TxtBuchInfo.Text = $"\"{buch.Titel}\"  |  {buch.AutorName}";

        // Standardwerte: heute bis in 28 Tagen
        DpAusgeliehenAm.SelectedDate  = DateTime.Today;
        DpRueckgabeBis.SelectedDate   = DateTime.Today.AddDays(28);
    }

    private void BtnBestaetigen_Click(object sender, RoutedEventArgs e)
    {
        if (DpAusgeliehenAm.SelectedDate == null || DpRueckgabeBis.SelectedDate == null)
        {
            MessageBox.Show("Bitte beide Daten auswaehlen.", "Hinweis",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DateTime von = DpAusgeliehenAm.SelectedDate.Value;
        DateTime bis = DpRueckgabeBis.SelectedDate.Value;

        if (bis <= von)
        {
            MessageBox.Show("Das Rueckgabedatum muss nach dem Ausleihdatum liegen.", "Hinweis",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = Database.BuchAusleihen(
            _buch.BuchId,
            Session.CurrentUser!.UserId,
            von,
            bis);

        if (result.Success)
        {
            Ausgeliehen = true;
            MessageBox.Show(
                $"Buch erfolgreich ausgeliehen!\n\nRueckgabe bis: {bis:dd.MM.yyyy}",
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