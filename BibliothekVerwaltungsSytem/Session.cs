namespace BibliothekVerwaltungsSytem;

/// <summary>
/// Hält den aktuell eingeloggten User für alle Seiten bereit.
/// </summary>
public static class Session
{
    public static User? CurrentUser { get; set; }

    public static void Logout() => CurrentUser = null;
}