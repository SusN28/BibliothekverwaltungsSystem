using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace BibliothekVerwaltungsSytem
{
    public partial class SuchePage 
    {
        private ObservableCollection<Database.UserAusleihe> _suchergebnisse = new();

        public SuchePage()
        {
            InitializeComponent();
            ErgebnisseDataGrid.ItemsSource = _suchergebnisse;
        }

        private void BtnInventar_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new InventarPage());

        private void BtnSuche_Click(object sender, RoutedEventArgs e)
        {
            // Schon auf der Seite
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _suchergebnisse.Clear();
            string suchbegriff = SearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(suchbegriff)) return;

            foreach (var buch in SucheAusleihen(suchbegriff))
                _suchergebnisse.Add(buch);
        }

        private ObservableCollection<Database.UserAusleihe> SucheAusleihen(string suchbegriff)
        {
            var ergebnisse = new ObservableCollection<Database.UserAusleihe>();
            try
            {
                using var connection = new MySql.Data.MySqlClient.MySqlConnection(Database.connectionString);
                connection.Open();

                using var cmd = new MySql.Data.MySqlClient.MySqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = @"
                    SELECT
                        b.titel,
                        CONCAT(a.vorname, ' ', a.nachname) AS autor,
                        b.erscheinungsjahr,
                        DATE_FORMAT(au.ausgeliehen_am, '%d.%m.%Y') AS ausgeliehen_am,
                        DATE_FORMAT(au.rueckgabe_bis,  '%d.%m.%Y') AS rueckgabe_bis
                    FROM ausleihen au
                    JOIN buecher b ON au.buch_id = b.buch_id
                    JOIN autoren a ON b.autor_id  = a.autor_id
                    WHERE au.user_id = @userId
                      AND au.status  = 'aktiv'
                      AND (b.titel LIKE @suche
                           OR CONCAT(a.vorname, ' ', a.nachname) LIKE @suche
                           OR b.isbn LIKE @suche)
                    ORDER BY b.titel";

                cmd.Parameters.AddWithValue("@userId", Session.CurrentUser!.UserId);
                cmd.Parameters.AddWithValue("@suche", "%" + suchbegriff + "%");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ergebnisse.Add(new Database.UserAusleihe
                    {
                        Autor            = reader.GetString("autor"),
                        Titel            = reader.GetString("titel"),
                        Erscheinungsjahr = reader.IsDBNull(reader.GetOrdinal("erscheinungsjahr"))
                                           ? ""
                                           : reader.GetInt16("erscheinungsjahr").ToString(),
                        AusgeliehenAm    = reader.GetString("ausgeliehen_am"),
                        AusgeliehenBis   = reader.GetString("rueckgabe_bis")
                    });
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"Suchfehler: {ex.Message}", "Fehler",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return ergebnisse;
        }
        private void BtnBuecher_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new BuecherPage());

    }
}
