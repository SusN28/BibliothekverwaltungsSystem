using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;
using System.Windows;

namespace BibliothekVerwaltungsSytem
{
    internal class Database
    {
        public const string connectionString =
            "server=mysql.pb.bib.de;uid=pba3h24age;pwd=2dMzNXSWA54s;database=pba3h24age";

        // ──────────────────────────────────────────────────────────────
        // LOGIN MIT BCRYPT
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Prüft username + password gegen die Datenbank.
        /// Das Passwort wird mit BCrypt.Verify() gegen den gespeicherten Hash geprüft.
        /// </summary>
        public static LoginResult LoginUser(string username, string password)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var cmd = new MySqlCommand(
                    @"SELECT user_id, username, password_hash, vorname, nachname, rolle
                      FROM users
                      WHERE username = @username AND aktiv = 1",
                    connection);

                cmd.Parameters.AddWithValue("@username", username);

                using var reader = cmd.ExecuteReader();

                // User nicht gefunden
                if (!reader.Read())
                    return new LoginResult { Success = false, Error = "Ungültige Anmeldedaten." };

                string storedHash = reader.GetString("password_hash");

                // BCrypt-Vergleich: plaintext-Eingabe vs. Hash aus DB
                if (!BCrypt.Net.BCrypt.Verify(password, storedHash))
                    return new LoginResult { Success = false, Error = "Ungültige Anmeldedaten." };

                var user = new User
                {
                    UserId   = reader.GetInt32("user_id"),
                    Username = reader.GetString("username"),
                    Vorname  = reader.GetString("vorname"),
                    Nachname = reader.GetString("nachname"),
                    Rolle    = reader.GetString("rolle")
                };

                return new LoginResult { Success = true, User = user };
            }
            catch (MySqlException ex)
            {
                return new LoginResult { Success = false, Error = $"Datenbankfehler: {ex.Message}" };
            }
        }

        // ──────────────────────────────────────────────────────────────
        // EINMALIGE MIGRATION: Plaintext → BCrypt
        // Nach dem ersten Ausführen können diese Methode wieder entfernen!
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Einmalig aufrufen (z.B. beim ersten App-Start) um alle
        /// Klartext-Passwörter in der DB durch BCrypt-Hashes zu ersetzen.
        /// </summary>
        public static void MigratePasswordsToBCrypt()
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            // Alle User laden
            var users = new List<(int id, string plaintext)>();
            using (var readCmd = new MySqlCommand("SELECT user_id, password_hash FROM users", connection))
            using (var reader = readCmd.ExecuteReader())
                while (reader.Read())
                    users.Add((reader.GetInt32("user_id"), reader.GetString("password_hash")));

            int migrated = 0;
            foreach (var (id, plaintext) in users)
            {
                // Bereits gehashte Passwörter überspringen ($2a$ oder $2b$ = BCrypt)
                if (plaintext.StartsWith("$2"))
                    continue;

                string hashed = BCrypt.Net.BCrypt.HashPassword(plaintext, workFactor: 12);

                using var updateCmd = new MySqlCommand(
                    "UPDATE users SET password_hash = @hash WHERE user_id = @id", connection);
                updateCmd.Parameters.AddWithValue("@hash", hashed);
                updateCmd.Parameters.AddWithValue("@id", id);
                updateCmd.ExecuteNonQuery();
                migrated++;
            }

            if (migrated > 0)
                MessageBox.Show($"{migrated} Passwort(e) erfolgreich migriert!", "Migration",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ──────────────────────────────────────────────────────────────
        // INVENTAR – jetzt mit userId statt hardcoded 'user'
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Lädt alle aktiven Ausleihen des eingeloggten Users anhand seiner user_id.
        /// </summary>
        public static ObservableCollection<UserAusleihe> LoadUserInventar(int userId)
        {
            var ausleihen = new ObservableCollection<UserAusleihe>();
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var cmd = new MySqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = @"
                    SELECT
                        b.titel,
                        CONCAT(a.vorname, ' ', a.nachname) AS autor,
                        b.erscheinungsjahr,
                        DATE_FORMAT(au.ausgeliehen_am, '%d.%m.%Y') AS ausgeliehen_am,
                        DATE_FORMAT(au.rueckgabe_bis,  '%d.%m.%Y') AS rueckgabe_bis
                    FROM ausleihen au
                    JOIN buecher b  ON au.buch_id  = b.buch_id
                    JOIN autoren a  ON b.autor_id   = a.autor_id
                    WHERE au.user_id = @userId
                      AND au.status  = 'aktiv'
                    ORDER BY au.ausgeliehen_am DESC";

                cmd.Parameters.AddWithValue("@userId", userId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ausleihen.Add(new UserAusleihe
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
            catch (MySqlException ex)
            {
                MessageBox.Show($"Datenbankfehler: {ex.Message}", "Fehler",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return ausleihen;
        }

        // ──────────────────────────────────────────────────────────────
        // DB-VERBINDUNGSTEST (unverändert)
        // ──────────────────────────────────────────────────────────────

        public static bool TestConnection()
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                MessageBox.Show("Verbindung zur Datenbank erfolgreich!", "Erfolg",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Verbindung fehlgeschlagen:\n{ex.Message}", "Fehler",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Model (unverändert)
        public class UserAusleihe
        {
            public string Autor            { get; set; } = "";
            public string Titel            { get; set; } = "";
            public string Erscheinungsjahr { get; set; } = "";
            public string AusgeliehenAm    { get; set; } = "";
            public string AusgeliehenBis   { get; set; } = "";
        }
        
        /// <summary>
        /// Erstellt einen neuen User mit BCrypt-gehaschtem Passwort.
        /// </summary>
        public static LoginResult CreateUser(string vorname, string nachname,
            string username, string email, string passwort, string rolle)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                // Prüfen ob Username bereits existiert
                using var checkCmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM users WHERE username = @username", connection);
                checkCmd.Parameters.AddWithValue("@username", username);
                long count = (long)checkCmd.ExecuteScalar();

                if (count > 0)
                    return new LoginResult { Success = false, Error = "Benutzername existiert bereits!" };

                // Passwort hashen
                string hash = BCrypt.Net.BCrypt.HashPassword(passwort, workFactor: 12);

                using var cmd = new MySqlCommand(@"
            INSERT INTO users (username, password_hash, vorname, nachname, email, rolle)
            VALUES (@username, @hash, @vorname, @nachname, @email, @rolle)", connection);

                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@hash",     hash);
                cmd.Parameters.AddWithValue("@vorname",  vorname);
                cmd.Parameters.AddWithValue("@nachname", nachname);
                cmd.Parameters.AddWithValue("@email",    string.IsNullOrEmpty(email) ? DBNull.Value : email);
                cmd.Parameters.AddWithValue("@rolle",    rolle);

                cmd.ExecuteNonQuery();
                return new LoginResult { Success = true };
            }
            catch (MySqlException ex)
            {
                return new LoginResult { Success = false, Error = $"Datenbankfehler: {ex.Message}" };
            }
        }
        
        // Model für die User-Übersicht
        public class UserInfo
        {
            public int    UserId   { get; set; }
            public string Username { get; set; } = "";
            public string Vorname  { get; set; } = "";
            public string Nachname { get; set; } = "";
            public string Email    { get; set; } = "";
            public string Rolle    { get; set; } = "";
        }
        /// <summary>
        /// Löscht einen User anhand seiner ID.
        /// Ausleihen werden durch CASCADE automatisch mitgelöscht.
        /// </summary>
        public static LoginResult DeleteUser(int userId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var cmd = new MySqlCommand(
                    "DELETE FROM users WHERE user_id = @userId", connection);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.ExecuteNonQuery();

                return new LoginResult { Success = true };
            }
            catch (MySqlException ex)
            {
                return new LoginResult { Success = false, Error = $"Datenbankfehler: {ex.Message}" };
            }
        }

        /// <summary>
        /// Lädt alle User aus der Datenbank (ohne password_hash).
        /// </summary>
        public static ObservableCollection<UserInfo> LoadAlleUser()
        {
            var liste = new ObservableCollection<UserInfo>();
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = new MySqlCommand(
                    "SELECT user_id, username, vorname, nachname, IFNULL(email,'') AS email, rolle FROM users ORDER BY user_id",
                    connection);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    liste.Add(new UserInfo
                    {
                        UserId   = reader.GetInt32("user_id"),
                        Username = reader.GetString("username"),
                        Vorname  = reader.GetString("vorname"),
                        Nachname = reader.GetString("nachname"),
                        Email    = reader.GetString("email"),
                        Rolle    = reader.GetString("rolle")
                    });
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Fehler beim Laden der User: {ex.Message}", "Fehler",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return liste;
        }
    }

    
}
