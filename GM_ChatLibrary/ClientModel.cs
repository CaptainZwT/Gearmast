using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GM_ChatLibrary
{
    public class ClientModel : INotifyPropertyChanged
    {
        private TcpClient _socket;

        private string _messageBoard, _UserList = "", _gamechat, _ttt_msg, _gamepartner = "", _hgmtxt="", _msgdisplay;
        private string _hmopenl = "", _hmclosedl = "";
        private bool _tttboard = false, _hmboard = false;
        private BitmapImage _hmfig = new BitmapImage(new Uri(@"/Resources/Games/HangMan/HM_6.png", UriKind.Relative));
        Dictionary<string, string> _TicTacToe = new Dictionary<string, string>()
            {
                { "x0y0", "k"},
                { "x1y0", "k"},
                { "x2y0", "k"},
                { "x0y1", "k"},
                { "x1y1", "k"},
                { "x2y1", "k"},
                { "x0y2", "k"},
                { "x1y2", "k"},
                { "x2y2", "k"}
            };

        public string ClientName, ChatState = "l";
        public bool game_challenge = false, playing_game = false;
        public string UserList
        {
            get { return _UserList; }
            set { _UserList = value; OnPropertyChanged(); }
        }
        public string MessageBoard
        {
            get { return _messageBoard; }
            set { _messageBoard = value; OnPropertyChanged(); }
        }

        public string MessageDisplay
        {
            get { return _msgdisplay; }
            set { _msgdisplay = value; OnPropertyChanged(); }
        }

        public string HgmText
        {
            get { return _hgmtxt; }
            set { _hgmtxt = value; OnPropertyChanged(); }
        }

        public string HGM_OpenL
        {
            get { return _hmopenl; }
            set { _hmopenl = value; OnPropertyChanged(); }
        }

        public string HGM_ClosedL
        {
            get { return _hmclosedl; }
            set { _hmclosedl = value; OnPropertyChanged(); }
        }

        public string Game_Chat
        {
            get { return _gamechat; }
            set { _gamechat = value; OnPropertyChanged(); }
        }

        public string Game_Board
        {
            get { return _gamechat; }
            set { _gamechat = value; OnPropertyChanged(); }
        }

        private string _currentMessage;
        public string CurrentMessage
        {
            get { return _currentMessage; }
            set { _currentMessage = value; OnPropertyChanged(); }
        }

        public string TTT_msg
        {
            get { return _ttt_msg; }
            set { _ttt_msg = value; OnPropertyChanged(); }
        }

        public Dictionary<string, string> TicTacToe
        {
            get { return _TicTacToe; }
            set{ _TicTacToe = value; OnPropertyChanged(); }
        }

        public bool TTTBoard
        {
            get { return _tttboard; }
            set { _tttboard = value; OnPropertyChanged(); }
        }

        public BitmapImage HMFigure
        {
            get { return _hmfig; }
            set { _hmfig = value; OnPropertyChanged(); }
        }


        public bool HMBoard
        {
            get { return _hmboard; }
            set { _hmboard = value; OnPropertyChanged(); }
        }

        public bool Connected
        {
            get { return _socket != null && _socket.Connected; }
        }

        public bool TTTg
        {
            get { return _tttboard; }
        }

        public void ChatSwitch(string l)
        {
            ChatState = l;
        }

        public void Connect()
        {
            _socket = new TcpClient("127.0.0.1", 8888);
            OnPropertyChanged("Connected");
            ClientName = _currentMessage;
            if ((ClientName[0] == '#') && (ClientName[1] == '('))
            {
                ClientName = ClientName.Substring(2);
            }
            Send();
            var thread = new Thread(GetMessage);
            thread.Start();
        }

        private void HM_Update(string word, string attemptedl, int lostl)
        {
            Dictionary<int, BitmapImage> hgmfig = new Dictionary<int, BitmapImage>()
            {
                { 0, new BitmapImage (new Uri(@"Resources/Games/HangMan/HM_0.png", UriKind.Relative))},
                { 1, new BitmapImage (new Uri(@"Resources/Games/HangMan/HM_1.png", UriKind.Relative))},
                { 2, new BitmapImage (new Uri(@"Resources/Games/HangMan/HM_2.png", UriKind.Relative))},
                { 3, new BitmapImage (new Uri(@"Resources/Games/HangMan/HM_3.png", UriKind.Relative))},
                { 4, new BitmapImage (new Uri(@"Resources/Games/HangMan/HM_4.png", UriKind.Relative))},
                { 5, new BitmapImage (new Uri(@"Resources/Games/HangMan/HM_5.png", UriKind.Relative))},
                { 6, new BitmapImage (new Uri(@"Resources/Games/HangMan/HM_6.png", UriKind.Relative))}
            };

                string abc = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

                // update letters
                string res = "";
                for (int i = 0; i < attemptedl.Length; i++)
                {
                    if (abc.Contains(attemptedl[i].ToString()))
                    {
                        abc = abc.Remove(abc.IndexOf(attemptedl[i]), 1);
                    }
                    res += (" " + attemptedl[i]);
                }

            HGM_ClosedL = res;
            HgmText = word;
            HMFigure = hgmfig[6-lostl];
            HGM_OpenL = abc;
        }

        private void TTT_Update(string pos, string mark)
        {
            _TicTacToe[pos] = mark;
        }

        public void TTT_Game(bool stat)
        {
            //
            _tttboard = stat;
            if (stat)
            {
                _TicTacToe = new Dictionary<string, string>() {
                { "x0y0", "k"},
                { "x1y0", "k"},
                { "x2y0", "k"},
                { "x0y1", "k"},
                { "x1y1", "k"},
                { "x2y1", "k"},
                { "x0y2", "k"},
                { "x1y2", "k"},
                { "x2y2", "k"}
                };
            }
        }

        public void TTT_Move(string n)
        { 
            // Where n's format = x0y0
            _socket.WriteString("g)ttt)" + n);
        }

        private void GetMessage()
        {
            while (true)
            {
                // Update the Chat as according to what Chat we are in. Currrently, "Game, Lobby"

                if (_UserList.Length < 5) // check we know who we are talking to, if not send a request for a new userlist
                {
                    _socket.WriteString("l(req)");
                }

                string msg = _socket.ReadString();
                if (msg[0] == '#') // lobby chat message
                {
                     MessageBoard += "\r\n" + msg.Substring(2);
                }
                else if (msg[0] == 'p') // game chat message
                {
                    string[] msgspl = msg.Substring(2).Split('(');
                    Game_Board += "\r\n [" + DateTime.Now + "] (" + msgspl[0] + ") " + msgspl[1];
                }
                else if (msg[0] == '$') // userlist message
                {
                    UserList = msg.Substring(2);
                }
                else if (msg[0] == 'g')
                {
                    if ((msg.Length==2) && (msg[1] == ')'))
                    {
                        Game_Board += ("\r\n [" + System.DateTime.Now + "]  (Server) Game ended.");
                        HMBoard = false;
                        TTTBoard = false;
                    }
                    else if (msg.Substring(2, 5) == "ttt(c") // tictactoe setup
                    {
                        Game_Board += ("\r\n [" + System.DateTime.Now + "]  (Server) You started a game of TicTacToe.");
                        TTT_Game(true);
                    }
                    else if (msg.Substring(2, 5) == "ttt()") // tictactoe update
                    {
                        string[] msgspl = msg.Substring(7).Split(')');
                        TTT_Update(msgspl[0], msgspl[1]);
                    }
                    else if (msg.Substring(2, 5) == "ttt(x") // tictactoe clean
                    {
                        TTT_Game(false);
                    }
                    else if (msg.Substring(2, 4) == "hgm(") // hangman request
                    {
                        string[] msgspl = msg.Substring(6).Split('(');
                        Game_Board += ("\r\n [" + System.DateTime.Now + "]  (Server) You started a game of Hangman against " + msgspl[0]);
                    }
                    else if (msg.Substring(2, 4) == "hgm)") // hangman update
                    {
                        HMBoard = true;
                        string[] msgspl = msg.Substring(6).Split(')');
                        HM_Update(msgspl[0], msgspl[1], Convert.ToInt32(msgspl[2]));
                    }
                }

            }
        }

        public void Send()
        {
            if ((_currentMessage.Length>2) && (_currentMessage[0] == '/'))
            {
                // commands
                if (_currentMessage.Substring(1, 3) == "try")
                {
                    // hangman letter attempt
                    _socket.WriteString("g)hgm)" + _currentMessage[5]);
                }
                else if (_currentMessage.Substring(1,3) == "hgm")
                {
                    // hangman request
                    string[] sp = _currentMessage.Substring(5).Split(' '); // sp[0] should now be the user, sp[1] will be the word
                    _socket.WriteString("h(" + sp[0] + "(" + sp[1]);
                }
                else if (_currentMessage.Substring(1, 3) == "ttt")
                {
                    // tictactoe request
                    _socket.WriteString("t(" + _currentMessage.Substring(5));
                    Game_Board += ("\r\n [" + System.DateTime.Now + "]  (Server) You started a game of TicTacToe against " + _gamepartner);
                }
                else if (_currentMessage.Substring(1, 4) == "game")
                {
                    //_socket.WriteString("p(" + ClientName + "(" + _currentMessage.Substring(6));
                    //Game_Board += "[" + DateTime.Now + "] (" + ClientName + ")" + _currentMessage.Substring(6);
                }
                else if ((_currentMessage[1] =='a') || (_currentMessage[1] == 'd'))
                {
                    _socket.WriteString("r(" + _currentMessage[1]);
                }
                else if ((_currentMessage[1] == '?') || (_currentMessage == "/help"))
                {
                    MessageBoard += "\n (Server) List of commands as follows:" +
                        "\r\n  /hm : send a hangman request in the form /hm opponent word" +
                        "\r\n /ttt : send a tictactoe request in the form /ttt opponent";
                }
                else
                {
                    MessageBoard += "\r\n (Server) That command was not understood, please refer to /? for list of valid commands.";
                }
            }
            else
            {
                _socket.WriteString("#(" +  _currentMessage);
                _currentMessage = "";
                CurrentMessage = "";
            }
        }

        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}