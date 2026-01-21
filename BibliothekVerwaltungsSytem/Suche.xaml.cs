using System.Windows;
using System.Windows.Controls;

namespace BibliothekVerwaltungsSytem
{
    public partial class SuchePage : Page
    {
        public SuchePage()
        {
            InitializeComponent();
        }

        private void BtnInventar_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new InventarPage());
        }

        private void BtnSuche_Click(object sender, RoutedEventArgs e)
        {
            // Schon auf der Seite
        }
    }
}