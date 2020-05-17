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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.ComponentModel;


namespace Gearmast_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*
        BitmapImage ttt_player,
        blank = new BitmapImage(new Uri(@"/Resources/Games/TicTacToe/K.png", UriKind.Relative)),
        ttt_o = new BitmapImage(new Uri(@"/Resources/Games/TicTacToe/O.png", UriKind.Relative)),
        ttt_x = new BitmapImage(new Uri(@"/Resources/Games/TicTacToe/X.png", UriKind.Relative));
        */

        // Chat related functions


        private void ChatSend_Click(object sender, RoutedEventArgs e)
        {
            MessageInput.Text = "";
        }


        private void Lobby_Chatswitch(object sender, RoutedEventArgs e)
        {
            Game_ChatBox.Visibility = Visibility.Hidden;
            Lobby_ChatBox.Visibility = Visibility.Visible;
        }

        private void Game_Chatswitch(object sender, RoutedEventArgs e)
        {
            Game_ChatBox.Visibility = Visibility.Visible;
            Lobby_ChatBox.Visibility = Visibility.Hidden;
        }
    }
}
