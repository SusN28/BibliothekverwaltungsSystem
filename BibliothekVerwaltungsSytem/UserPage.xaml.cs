using System.Windows;
using System.Windows.Controls;

namespace BibliothekVerwaltungsSytem
{
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();

            // Vorname und Nachname aus der Session laden
            if (Session.CurrentUser != null)
                TxtWillkommen.Text = $"Willkommen, {Session.CurrentUser.Vorname} {Session.CurrentUser.Nachname}!";
        }

        private void WeiterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new InventarPage());
        }
    }
}