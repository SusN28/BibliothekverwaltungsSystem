using System.Windows;
using System.Windows.Controls;

namespace BibliothekVerwaltungsSytem
{
    public partial class InventarPage : Page
    {
        public InventarPage()
        {
            InitializeComponent();
            LoadInventar();
        }

        private void LoadInventar()
        {
            // Daten aus der Datenbank laden
            var ausleihen = Database.LoadUserInventar();
            
            // An DataGrid binden
            dgBuecher.ItemsSource = ausleihen;
        }

        private void BtnInventar_Click(object sender, RoutedEventArgs e)
        {
            // Schon auf der Seite
        }

        private void BtnSuche_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SuchePage());
        }
    }
}