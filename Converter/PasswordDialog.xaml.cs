using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Converter
{
    /// <summary>
    /// Interaction logic for PasswordDialog.xaml
    /// </summary>
    public partial class PasswordDialog : Window
    {
        public string Password { get; private set; }
        public PasswordDialog()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            // Set the Password property and close the window
            Password = passwordBox.Password;
            DialogResult = true;
        }

        public void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Close the window without setting the password
            DialogResult = false;
        }
    }
}
