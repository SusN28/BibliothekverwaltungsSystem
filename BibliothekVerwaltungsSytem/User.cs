namespace BibliothekVerwaltungsSytem;

public class User
{
    public int    UserId   { get; set; }
    public string Username { get; set; } = "";
    public string Vorname  { get; set; } = "";
    public string Nachname { get; set; } = "";
    public string Rolle    { get; set; } = "user"; // "user" oder "admin"
}