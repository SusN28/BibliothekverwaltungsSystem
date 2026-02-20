using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace BibliothekVerwaltungsSytem
{
    public partial class SuchePage : Page
    {
        private ObservableCollection<UserAusleihe> suchergebnisse = new ObservableCollection<UserAusleihe>();

        public SuchePage()
        {
            InitializeComponent();
            // DataGrid für Ergebnisse (füge das zu deinem XAML hinzu, siehe unten)
            ErgebnisseDataGrid.ItemsSource = suchergebnisse;
        }

        private void BtnInventar_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new InventarPage());
        }

        private void BtnSuche_Click(object sender, RoutedEventArgs e)
        {
            // Schon auf der Seite
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string suchbegriff = SearchTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(suchbegriff))
            {
                suchergebnisse.Clear();
                return;
            }

            // Suche nur in den ausgeliehenen Büchern des Users "user" 
            suchergebnisse.Clear();
            var gefundeneBuecher = SucheAusleihen(suchbegriff);
            foreach (var buch in gefundeneBuecher)
            {
                suchergebnisse.Add(buch);
            }
        }

        private ObservableCollection<UserAusleihe> SucheAusleihen(string suchbegriff)
        {
            ObservableCollection<UserAusleihe> ergebnisse = new ObservableCollection<UserAusleihe>();
            
            try
            {
                using (var connection = new MySql.Data.MySqlClient.MySqlConnection(Database.connectionString))
                {
                    connection.Open();
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = @"
                            SELECT 
                                b.titel,
                                CONCAT(a.vorname, ' ', a.nachname) AS autor,
                                b.erscheinungsjahr,
                                DATE_FORMAT(au.ausgeliehen_am, '%d.%m.%Y') AS ausgeliehen_am,
                                DATE_FORMAT(au.rueckgabe_bis, '%d.%m.%Y') AS rueckgabe_bis
                            FROM ausleihen au
                            JOIN buecher b ON au.buch_id = b.buch_id
                            JOIN autoren a ON b.autor_id = a.autor_id
                            JOIN users u ON au.user_id = u.user_id
                            WHERE u.username = 'user' 
                              AND au.status = 'aktiv'
                              AND (b.titel LIKE @suche 
                                   OR CONCAT(a.vorname, ' ', a.nachname) LIKE @suche
                                   OR b.isbn LIKE @suche)
                            ORDER BY b.titel";
                        
                        cmd.Parameters.AddWithValue("@suche", "%" + suchbegriff + "%");
                        
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ergebnisse.Add(new UserAusleihe
                                {
                                    Autor = reader.GetString("autor"),
                                    Titel = reader.GetString("titel"),
                                    Erscheinungsjahr = reader.IsDBNull(reader.GetOrdinal("erscheinungsjahr")) 
                                        ? "" 
                                        : reader.GetInt16("erscheinungsjahr").ToString(),
                                    AusgeliehenAm = reader.GetString("ausgeliehen_am"),
                                    AusgeliehenBis = reader.GetString("rueckgabe_bis")
                                });
                            }
                        }
                    }
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"Suchfehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            return ergebnisse;
        }
    }
}
