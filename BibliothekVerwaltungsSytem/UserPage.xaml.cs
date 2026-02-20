using System.Windows;
using System.Windows.Controls;

namespace BibliothekVerwaltungsSytem
{
    public partial class UserPage : Page
    {
        
        public UserPage()
        {
            InitializeComponent();
        }

        private void WeiterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new InventarPage());
        }
    }
}