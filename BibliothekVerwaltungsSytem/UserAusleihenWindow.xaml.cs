using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BibliothekVerwaltungsSytem;

public partial class UserAusleihenWindow : Window
{
    private readonly int _userId;
    private ObservableCollection<Database.AusleiheInfo> _ausleihen = new();

    public UserAusleihenWindow(Database.UserInfo user)
    {
        InitializeComponent();

        _userId = user.UserId;
        TxtUserInfo.Text = $"{user.Vorname} {user.Nachname}  |  @{user.Username}";

        LadeAusleihen();
    }

    private void LadeAusleihen()
    {
        _ausleihen = Database.LoadAusleihenVonUser(_userId);
        AusleihenDataGrid.ItemsSource = _ausleihen;
        TxtAnzahlAusleihen.Text = _ausleihen.Count.ToString();
    }

    private void BtnZurueckgeben_Click(object sender, RoutedEventArgs e)
    {
        int ausleiheId = (int)((Button)sender).Tag;
        var ausleihe = _ausleihen.FirstOrDefault(a => a.AusleiheId == ausleiheId);
        if (ausleihe == null) return;

        var bestaetigung = MessageBox.Show(
            $"Buch \"{ausleihe.Titel}\" als zurueckgegeben markieren?",
            "Buch zurueckgeben",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (bestaetigung != MessageBoxResult.Yes) return;

        var result = Database.BuchZurueckgeben(ausleiheId, ausleihe.BuchId);

        if (result.Success)
        {
            _ausleihen.Remove(ausleihe);
            TxtAnzahlAusleihen.Text = _ausleihen.Count.ToString();
            MessageBox.Show($"Buch \"{ausleihe.Titel}\" wurde erfolgreich zurueckgegeben.",
                "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show($"Fehler: {result.Error}", "Fehler",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnSchliessen_Click(object sender, RoutedEventArgs e)
        => Close();
}