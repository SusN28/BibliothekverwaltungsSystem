using System.Windows;
using System.Windows.Controls;

namespace BibliothekVerwaltungsSytem
{
    public partial class InventarPage : Page
    {
        public InventarPage()
        {
            InitializeComponent();
            LoadDemoData();
        }

        private void LoadDemoData()
        {
            // Demo-Daten für Test
            var buecher = new[]
            {
                new { Autor = "J.K. Rowling", Titel = "Harry Potter und der Stein der Weisen", Erscheinungsjahr = "1997", AusgeliehenAm = "10.01.2026", AusgeliehenBis = "24.01.2026" },
                new { Autor = "George Orwell", Titel = "1984", Erscheinungsjahr = "1949", AusgeliehenAm = "12.01.2026", AusgeliehenBis = "26.01.2026" }
            };
            dgBuecher.ItemsSource = buecher;
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