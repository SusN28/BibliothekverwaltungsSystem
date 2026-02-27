namespace BibliothekVerwaltungsSytem;

public class LoginResult
{
    public bool   Success { get; set; }
    public User?  User    { get; set; }
    public string Error   { get; set; } = "";
}