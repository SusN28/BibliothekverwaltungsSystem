using System;
using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;
using System.Windows;

namespace BibliothekVerwaltungsSytem
{
    internal class Database
    {
        public const string connectionString = "server=mysql.pb.bib.de;uid=pba3h24age;pwd=2dMzNXSWA54s;database=pba3h24age";

        /// <summary>
        /// Lädt alle ausgeliehenen Bücher des Users "user"
        /// </summary>
        public static ObservableCollection<UserAusleihe> LoadUserInventar()
        {
            ObservableCollection<UserAusleihe> ausleihen = new ObservableCollection<UserAusleihe>();
            
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = connection;
                    
                    // SQL: Hole alle Ausleihen vom User "user" mit Buch-Details
                    cmd.CommandText = @"SELECT 
                                          b.titel,
                                          CONCAT(a.vorname, ' ', a.nachname) AS autor,
                                          b.erscheinungsjahr,
                                          DATE_FORMAT(au.ausgeliehen_am, '%d.%m.%Y') AS ausgeliehen_am,
                                          DATE_FORMAT(au.rueckgabe_bis, '%d.%m.%Y') AS rueckgabe_bis
                                       FROM ausleihen au
                                       JOIN buecher b ON au.buch_id = b.buch_id
                                       JOIN autoren a ON b.autor_id = a.autor_id
                                       JOIN users u ON au.user_id = u.user_id
                                       WHERE u.username = 'user' AND au.status = 'aktiv'
                                       ORDER BY au.ausgeliehen_am DESC";

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ausleihen.Add(new UserAusleihe
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
            catch (MySqlException ex)
            {
                MessageBox.Show($"Datenbankfehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            return ausleihen;
        }

        /// <summary>
        /// Testet die Datenbankverbindung
        /// </summary>
        public static bool TestConnection()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MessageBox.Show("Verbindung zur Datenbank erfolgreich!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Verbindung fehlgeschlagen:\n{ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }

    // Model für die Ausleihen-Anzeige
    public class UserAusleihe
    {
        public string Autor { get; set; } = "";
        public string Titel { get; set; } = "";
        public string Erscheinungsjahr { get; set; } = "";
        public string AusgeliehenAm { get; set; } = "";
        public string AusgeliehenBis { get; set; } = "";
    }
}
