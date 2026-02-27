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
            // Bücher des eingeloggten Users laden (kein hardcoded 'user' mehr)
            var ausleihen = Database.LoadUserInventar(Session.CurrentUser!.UserId);
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
        private void BtnBuecher_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new BuecherPage());

    }
}