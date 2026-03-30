# Bibliotheksverwaltungssystem

Eine Desktop-Anwendung zur Verwaltung einer Bibliothek, entwickelt mit **WPF (.NET)** und **MySQL**.

## Funktionen

### Benutzer
- Login mit verschlüsseltem Passwort (BCrypt)
- Übersicht der eigenen ausgeliehenen Bücher
- Bücher suchen und ausleihen
- Warnung bei überfälligen Ausleihen (Ausleihen neuer Bücher gesperrt)

### Admin
- User erstellen, bearbeiten und löschen
- Bücher verwalten (hinzufügen, bearbeiten, löschen)
- Autoren hinzufügen
- Exemplaranzahl erhöhen
- Ausleihen pro User verwalten und Bücher zurücknehmen

## Technologien

| Technologie | Verwendung |
|---|---|
| C# / WPF | Desktop-Oberfläche |
| MySQL | Datenbank |
| BCrypt.Net | Passwort-Hashing |
| MySql.Data | Datenbankverbindung |

## Voraussetzungen

- .NET 6 oder höher
- MySQL-Server (Zugangsdaten in `Database.cs` anpassen)
- NuGet-Pakete: `BCrypt.Net-Next`, `MySql.Data`
