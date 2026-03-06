using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;
using System.Windows;

namespace BibliothekVerwaltungsSytem
{
    public class Database
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
        // DB-VERBINDUNGSTEST 
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
/// Aktualisiert die Daten eines Users.
/// Passwort wird nur geaendert wenn nicht leer.
/// </summary>
    public static LoginResult UpdateUser(int userId, string vorname, string nachname,
        string username, string email, string neuesPasswort, string rolle)
    {
        try
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            // Prüfen ob Username schon von jemand anderem verwendet wird
            using var checkCmd = new MySqlCommand(
                "SELECT COUNT(*) FROM users WHERE username = @username AND user_id != @userId",
                connection);
            checkCmd.Parameters.AddWithValue("@username", username);
            checkCmd.Parameters.AddWithValue("@userId", userId);
            if ((long)checkCmd.ExecuteScalar() > 0)
                return new LoginResult { Success = false, Error = "Benutzername wird bereits verwendet!" };

            // SQL je nachdem ob Passwort geändert wird oder nicht
            string sql = string.IsNullOrEmpty(neuesPasswort)
                ? @"UPDATE users SET vorname=@vorname, nachname=@nachname,
                    username=@username, email=@email, rolle=@rolle
                    WHERE user_id=@userId"
                : @"UPDATE users SET vorname=@vorname, nachname=@nachname,
                    username=@username, email=@email, rolle=@rolle,
                    password_hash=@hash
                    WHERE user_id=@userId";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@vorname",  vorname);
            cmd.Parameters.AddWithValue("@nachname", nachname);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@email",    string.IsNullOrEmpty(email) ? DBNull.Value : email);
            cmd.Parameters.AddWithValue("@rolle",    rolle);
            cmd.Parameters.AddWithValue("@userId",   userId);

            if (!string.IsNullOrEmpty(neuesPasswort))
                cmd.Parameters.AddWithValue("@hash",
                    BCrypt.Net.BCrypt.HashPassword(neuesPasswort, workFactor: 12));

            cmd.ExecuteNonQuery();
            return new LoginResult { Success = true };
        }
        catch (MySqlException ex)
        {
            return new LoginResult { Success = false, Error = $"Datenbankfehler: {ex.Message}" };
        }
    }

        // ── Modelle ────────────────────────────────────────────────
public class BuchInfo
{
    public int    BuchId          { get; set; }
    public string Titel           { get; set; } = "";
    public string Isbn            { get; set; } = "";
    public int    AutorId         { get; set; }
    public string AutorName       { get; set; } = "";
    public int?   KategorieId     { get; set; }
    public string KategorieName   { get; set; } = "";
    public string Erscheinungsjahr { get; set; } = "";
    public string Verlag          { get; set; } = "";
    public string Seitenzahl      { get; set; } = "";
    public string Sprache         { get; set; } = "Deutsch";
    public int    AnzahlExemplare { get; set; }
    public int    Verfuegbar      { get; set; }
    public string Beschreibung    { get; set; } = "";
}

public class AutorInfo
{
    public int    AutorId { get; set; }
    public string Name    { get; set; } = "";
}

public class KategorieInfo
{
    public int    KategorieId { get; set; }
    public string Name        { get; set; } = "";
}

// ── Methoden ───────────────────────────────────────────────

public static ObservableCollection<BuchInfo> LoadAlleBuecher()
{
    var liste = new ObservableCollection<BuchInfo>();
    try
    {
        using var connection = new MySqlConnection(connectionString);
        connection.Open();
        using var cmd = new MySqlCommand(@"
            SELECT b.buch_id, b.titel, IFNULL(b.isbn,'') AS isbn,
                   b.autor_id,
                   CONCAT(a.vorname, ' ', a.nachname) AS autor_name,
                   b.kategorie_id,
                   IFNULL(k.name,'') AS kategorie_name,
                   IFNULL(b.erscheinungsjahr,'') AS erscheinungsjahr,
                   IFNULL(b.verlag,'') AS verlag,
                   IFNULL(b.seitenzahl,'') AS seitenzahl,
                   IFNULL(b.sprache,'Deutsch') AS sprache,
                   b.anzahl_exemplare, b.verfuegbar,
                   IFNULL(b.beschreibung,'') AS beschreibung
            FROM buecher b
            JOIN autoren a ON b.autor_id = a.autor_id
            LEFT JOIN kategorien k ON b.kategorie_id = k.kategorie_id
            ORDER BY b.titel", connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            liste.Add(new BuchInfo
            {
                BuchId           = reader.GetInt32("buch_id"),
                Titel            = reader.GetString("titel"),
                Isbn             = reader.GetString("isbn"),
                AutorId          = reader.GetInt32("autor_id"),
                AutorName        = reader.GetString("autor_name"),
                KategorieId      = reader.IsDBNull(reader.GetOrdinal("kategorie_id"))
                                   ? null : reader.GetInt32("kategorie_id"),
                KategorieName    = reader.GetString("kategorie_name"),
                Erscheinungsjahr = reader.GetString("erscheinungsjahr"),
                Verlag           = reader.GetString("verlag"),
                Seitenzahl       = reader.GetString("seitenzahl"),
                Sprache          = reader.GetString("sprache"),
                AnzahlExemplare  = reader.GetInt32("anzahl_exemplare"),
                Verfuegbar       = reader.GetInt32("verfuegbar"),
                Beschreibung     = reader.GetString("beschreibung")
            });
        }
    }
    catch (MySqlException ex)
    {
        MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    return liste;
}

public static ObservableCollection<AutorInfo> LoadAlleAutoren()
{
    var liste = new ObservableCollection<AutorInfo>();
    try
    {
        using var connection = new MySqlConnection(connectionString);
        connection.Open();
        using var cmd = new MySqlCommand(
            "SELECT autor_id, CONCAT(vorname,' ',nachname) AS name FROM autoren ORDER BY nachname",
            connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            liste.Add(new AutorInfo
            {
                AutorId = reader.GetInt32("autor_id"),
                Name    = reader.GetString("name")
            });
    }
    catch (MySqlException ex)
    {
        MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    return liste;
}

public static ObservableCollection<KategorieInfo> LoadAlleKategorien()
{
    var liste = new ObservableCollection<KategorieInfo>();
    try
    {
        using var connection = new MySqlConnection(connectionString);
        connection.Open();
        using var cmd = new MySqlCommand(
            "SELECT kategorie_id, name FROM kategorien ORDER BY name", connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            liste.Add(new KategorieInfo
            {
                KategorieId = reader.GetInt32("kategorie_id"),
                Name        = reader.GetString("name")
            });
    }
    catch (MySqlException ex)
    {
        MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    return liste;
}

public static LoginResult CreateBuch(string titel, string isbn, int autorId, int? kategorieId,
    string jahr, string verlag, string seiten, string sprache, int exemplare, string beschreibung)
{
    try
    {
        using var connection = new MySqlConnection(connectionString);
        connection.Open();
        using var cmd = new MySqlCommand(@"
            INSERT INTO buecher
                (titel, isbn, autor_id, kategorie_id, erscheinungsjahr,
                 verlag, seitenzahl, sprache, anzahl_exemplare, verfuegbar, beschreibung)
            VALUES
                (@titel, @isbn, @autorId, @kategorieId, @jahr,
                 @verlag, @seiten, @sprache, @exemplare, @exemplare, @beschreibung)",
            connection);

        cmd.Parameters.AddWithValue("@titel",      titel);
        cmd.Parameters.AddWithValue("@isbn",       string.IsNullOrEmpty(isbn) ? DBNull.Value : isbn);
        cmd.Parameters.AddWithValue("@autorId",    autorId);
        cmd.Parameters.AddWithValue("@kategorieId", kategorieId.HasValue ? kategorieId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@jahr",       string.IsNullOrEmpty(jahr) ? DBNull.Value : jahr);
        cmd.Parameters.AddWithValue("@verlag",     string.IsNullOrEmpty(verlag) ? DBNull.Value : verlag);
        cmd.Parameters.AddWithValue("@seiten",     string.IsNullOrEmpty(seiten) ? DBNull.Value : seiten);
        cmd.Parameters.AddWithValue("@sprache",    string.IsNullOrEmpty(sprache) ? "Deutsch" : sprache);
        cmd.Parameters.AddWithValue("@exemplare",  exemplare);
        cmd.Parameters.AddWithValue("@beschreibung", string.IsNullOrEmpty(beschreibung) ? DBNull.Value : beschreibung);

        cmd.ExecuteNonQuery();
        return new LoginResult { Success = true };
    }
    catch (MySqlException ex)
    {
        return new LoginResult { Success = false, Error = $"Datenbankfehler: {ex.Message}" };
    }
}

public static LoginResult UpdateBuch(int buchId, string titel, string isbn, int autorId,
    int? kategorieId, string jahr, string verlag, string seiten,
    string sprache, int exemplare, string beschreibung)
{
    try
    {
        using var connection = new MySqlConnection(connectionString);
        connection.Open();
        using var cmd = new MySqlCommand(@"
            UPDATE buecher SET
                titel=@titel, isbn=@isbn, autor_id=@autorId,
                kategorie_id=@kategorieId, erscheinungsjahr=@jahr,
                verlag=@verlag, seitenzahl=@seiten, sprache=@sprache,
                anzahl_exemplare=@exemplare, beschreibung=@beschreibung
            WHERE buch_id=@buchId", connection);

        cmd.Parameters.AddWithValue("@titel",      titel);
        cmd.Parameters.AddWithValue("@isbn",       string.IsNullOrEmpty(isbn) ? DBNull.Value : isbn);
        cmd.Parameters.AddWithValue("@autorId",    autorId);
        cmd.Parameters.AddWithValue("@kategorieId", kategorieId.HasValue ? kategorieId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@jahr",       string.IsNullOrEmpty(jahr) ? DBNull.Value : jahr);
        cmd.Parameters.AddWithValue("@verlag",     string.IsNullOrEmpty(verlag) ? DBNull.Value : verlag);
        cmd.Parameters.AddWithValue("@seiten",     string.IsNullOrEmpty(seiten) ? DBNull.Value : seiten);
        cmd.Parameters.AddWithValue("@sprache",    string.IsNullOrEmpty(sprache) ? "Deutsch" : sprache);
        cmd.Parameters.AddWithValue("@exemplare",  exemplare);
        cmd.Parameters.AddWithValue("@beschreibung", string.IsNullOrEmpty(beschreibung) ? DBNull.Value : beschreibung);
        cmd.Parameters.AddWithValue("@buchId",     buchId);

        cmd.ExecuteNonQuery();
        return new LoginResult { Success = true };
    }
    catch (MySqlException ex)
    {
        return new LoginResult { Success = false, Error = $"Datenbankfehler: {ex.Message}" };
    }
}

public static LoginResult DeleteBuch(int buchId)
{
    try
    {
        using var connection = new MySqlConnection(connectionString);
        connection.Open();
        using var cmd = new MySqlCommand(
            "DELETE FROM buecher WHERE buch_id = @buchId", connection);
        cmd.Parameters.AddWithValue("@buchId", buchId);
        cmd.ExecuteNonQuery();
        return new LoginResult { Success = true };
    }
    catch (MySqlException ex)
    {
        return new LoginResult { Success = false, Error = $"Datenbankfehler: {ex.Message}" };
    }
}


/// <summary>
/// Laedt alle Buecher mit mindestens 1 verfuegbarem Exemplar.
/// </summary>
public static ObservableCollection<BuchInfo> LoadVerfuegbareBuecher()
{
    var liste = new ObservableCollection<BuchInfo>();
    try
    {
        using var connection = new MySqlConnection(connectionString);
        connection.Open();
        using var cmd = new MySqlCommand(@"
            SELECT b.buch_id, b.titel, IFNULL(b.isbn,'') AS isbn,
                   b.autor_id,
                   CONCAT(a.vorname, ' ', a.nachname) AS autor_name,
                   b.kategorie_id,
                   IFNULL(k.name,'') AS kategorie_name,
                   IFNULL(b.erscheinungsjahr,'') AS erscheinungsjahr,
                   IFNULL(b.verlag,'') AS verlag,
                   IFNULL(b.seitenzahl,'') AS seitenzahl,
                   IFNULL(b.sprache,'Deutsch') AS sprache,
                   b.anzahl_exemplare, b.verfuegbar,
                   IFNULL(b.beschreibung,'') AS beschreibung
            FROM buecher b
            JOIN autoren a ON b.autor_id = a.autor_id
            LEFT JOIN kategorien k ON b.kategorie_id = k.kategorie_id
            ORDER BY b.titel", connection);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            liste.Add(new BuchInfo
            {
                BuchId           = reader.GetInt32("buch_id"),
                Titel            = reader.GetString("titel"),
                Isbn             = reader.GetString("isbn"),
                AutorId          = reader.GetInt32("autor_id"),
                AutorName        = reader.GetString("autor_name"),
                KategorieId      = reader.IsDBNull(reader.GetOrdinal("kategorie_id"))
                                   ? null : reader.GetInt32("kategorie_id"),
                KategorieName    = reader.GetString("kategorie_name"),
                Erscheinungsjahr = reader.GetString("erscheinungsjahr"),
                Verlag           = reader.GetString("verlag"),
                Seitenzahl       = reader.GetString("seitenzahl"),
                Sprache          = reader.GetString("sprache"),
                AnzahlExemplare  = reader.GetInt32("anzahl_exemplare"),
                Verfuegbar       = reader.GetInt32("verfuegbar"),
                Beschreibung     = reader.GetString("beschreibung")
            });
        }
    }
    catch (MySqlException ex)
    {
        MessageBox.Show($"Fehler: {ex.Message}", "Fehler",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
    return liste;
}

/// <summary>
/// Erstellt eine neue Ausleihe und reduziert verfuegbar um 1.
/// </summary>
public static LoginResult BuchAusleihen(int buchId, int userId, DateTime von, DateTime bis)
{
    try
    {
        using var connection = new MySqlConnection(connectionString);
        connection.Open();

        // Sicherheitscheck: noch verfuegbar?
        using var checkCmd = new MySqlCommand(
            "SELECT verfuegbar FROM buecher WHERE buch_id = @buchId", connection);
        checkCmd.Parameters.AddWithValue("@buchId", buchId);
        int verfuegbar = Convert.ToInt32(checkCmd.ExecuteScalar());

        if (verfuegbar < 1)
            return new LoginResult { Success = false, Error = "Dieses Buch ist aktuell nicht verfuegbar." };

        // Ausleihe eintragen
        using var insertCmd = new MySqlCommand(@"
            INSERT INTO ausleihen (buch_id, user_id, ausgeliehen_am, rueckgabe_bis, status)
            VALUES (@buchId, @userId, @von, @bis, 'aktiv')", connection);

        insertCmd.Parameters.AddWithValue("@buchId", buchId);
        insertCmd.Parameters.AddWithValue("@userId", userId);
        insertCmd.Parameters.AddWithValue("@von",    von.ToString("yyyy-MM-dd"));
        insertCmd.Parameters.AddWithValue("@bis",    bis.ToString("yyyy-MM-dd"));
        insertCmd.ExecuteNonQuery();

        // Verfuegbar -1
        using var updateCmd = new MySqlCommand(
            "UPDATE buecher SET verfuegbar = verfuegbar - 1 WHERE buch_id = @buchId", connection);
        updateCmd.Parameters.AddWithValue("@buchId", buchId);
        updateCmd.ExecuteNonQuery();

        return new LoginResult { Success = true };
    }
    catch (MySqlException ex)
    {
        return new LoginResult { Success = false, Error = $"Datenbankfehler: {ex.Message}" };
    }
}


// ── Model ──────────────────────────────────────────────────
public class AusleiheInfo
{
    public int    AusleiheId     { get; set; }
    public int    BuchId         { get; set; }
    public string Titel          { get; set; } = "";
    public string Autor          { get; set; } = "";
    public string AusgeliehenAm  { get; set; } = "";
    public string RueckgabeBis   { get; set; } = "";
    public bool   IstUeberfaellig { get; set; }
}

// ── Methoden ───────────────────────────────────────────────

/// <summary>
/// Laedt alle aktiven Ausleihen eines bestimmten Users.
/// </summary>
public static ObservableCollection<AusleiheInfo> LoadAusleihenVonUser(int userId)
{
    var liste = new ObservableCollection<AusleiheInfo>();
    try
    {
        using var connection = new MySqlConnection(connectionString);
        connection.Open();
        using var cmd = new MySqlCommand(@"
            SELECT au.ausleihe_id, au.buch_id,
                   b.titel,
                   CONCAT(a.vorname, ' ', a.nachname) AS autor,
                   DATE_FORMAT(au.ausgeliehen_am, '%d.%m.%Y') AS ausgeliehen_am,
                   DATE_FORMAT(au.rueckgabe_bis,  '%d.%m.%Y') AS rueckgabe_bis,
                   au.rueckgabe_bis AS rueckgabe_bis_raw
            FROM ausleihen au
            JOIN buecher b ON au.buch_id  = b.buch_id
            JOIN autoren a ON b.autor_id  = a.autor_id
            WHERE au.user_id = @userId
              AND au.status IN ('aktiv', 'ueberfaellig')
            ORDER BY au.rueckgabe_bis ASC", connection);

        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            liste.Add(new AusleiheInfo
            {
                AusleiheId      = reader.GetInt32("ausleihe_id"),
                BuchId          = reader.GetInt32("buch_id"),
                Titel           = reader.GetString("titel"),
                Autor           = reader.GetString("autor"),
                AusgeliehenAm   = reader.GetString("ausgeliehen_am"),
                RueckgabeBis    = reader.GetString("rueckgabe_bis"),
                IstUeberfaellig = reader.GetDateTime("rueckgabe_bis_raw") < System.DateTime.Today
            });
        }
    }
    catch (MySqlException ex)
    {
        MessageBox.Show($"Fehler: {ex.Message}", "Fehler",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
    return liste;
}

/// <summary>
/// Markiert eine Ausleihe als zurueckgegeben und erhoeht verfuegbar um 1.
/// </summary>
public static LoginResult BuchZurueckgeben(int ausleiheId, int buchId)
{
    try
    {
        using var connection = new MySqlConnection(connectionString);
        connection.Open();

        // Ausleihe als zurueckgegeben markieren
        using var updateAusleihe = new MySqlCommand(@"
            UPDATE ausleihen
            SET status            = 'zurueckgegeben',
                zurueckgegeben_am = CURDATE()
            WHERE ausleihe_id = @ausleiheId", connection);
        updateAusleihe.Parameters.AddWithValue("@ausleiheId", ausleiheId);
        updateAusleihe.ExecuteNonQuery();

        // Verfuegbar + 1
        using var updateBuch = new MySqlCommand(@"
            UPDATE buecher
            SET verfuegbar = verfuegbar + 1
            WHERE buch_id = @buchId", connection);
        updateBuch.Parameters.AddWithValue("@buchId", buchId);
        updateBuch.ExecuteNonQuery();

        return new LoginResult { Success = true };
    }
    catch (MySqlException ex)
    {
        return new LoginResult { Success = false, Error = $"Datenbankfehler: {ex.Message}" };
    }
}

/// <summary>
/// Fuegt einen neuen Autor in die autoren-Tabelle ein.
/// </summary>
public static LoginResult CreateAutor(string vorname, string nachname,
    string geburtsjahr, string nationalitaet, string biografie)
{
    try
    {
        using var connection = new MySqlConnection(connectionString);
        connection.Open();

        using var cmd = new MySqlCommand(@"
            INSERT INTO autoren (vorname, nachname, geburtsjahr, nationalitaet, biografie)
            VALUES (@vorname, @nachname, @geburtsjahr, @nationalitaet, @biografie)",
            connection);

        cmd.Parameters.AddWithValue("@vorname",      string.IsNullOrEmpty(vorname) ? DBNull.Value : vorname);
        cmd.Parameters.AddWithValue("@nachname",     nachname);
        cmd.Parameters.AddWithValue("@geburtsjahr",  string.IsNullOrEmpty(geburtsjahr) ? DBNull.Value : geburtsjahr);
        cmd.Parameters.AddWithValue("@nationalitaet", string.IsNullOrEmpty(nationalitaet) ? DBNull.Value : nationalitaet);
        cmd.Parameters.AddWithValue("@biografie",    string.IsNullOrEmpty(biografie) ? DBNull.Value : biografie);

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
