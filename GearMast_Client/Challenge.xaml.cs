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

namespace Gearmast_Client
{
    /// <summary>
    /// Interaction logic for Challenge.xaml
    /// </summary>
    public partial class Challenge : Window
    {
        string challenge = " has challenged you to a game of ";
        public bool result;
        public Challenge(string opponent, string game, bool r)
        {
            challenge = opponent + challenge + game;
            r = result;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
