using GM_ChatLibrary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System;

namespace Gearmast_Client.ViewModels
{
    /// <summary>
    /// Adaptive code only. You should only see things here that adapt
    /// the Model to the view. This is an abstraction of the Model for 
    /// the express use by the View.
    /// </summary>
    class ClientViewModel : INotifyPropertyChanged
    {
        private BitmapImage ttt_x = new BitmapImage(new Uri("@/Resources/Games/TicTacToe/X.png", UriKind.Relative));
        private BitmapImage ttt_o = new BitmapImage(new Uri("@/Resources/Games/TicTacToe/O.png", UriKind.Relative));
        private BitmapImage ttt_k = new BitmapImage(new Uri("@/Resources/Games/TicTacToe/K.png", UriKind.Relative));
        #region Properties
        //Elements bound to by the view
        public string Message
        {
            get { return _clientModel.CurrentMessage; }
            set { _clientModel.CurrentMessage = value; NotifyPropertyChanged(); }
        }

        public string Game_Board
        {
            get { return _clientModel.Game_Board; }
            set { _clientModel.Game_Board = value; NotifyPropertyChanged(); }
        }

        public string NameInput
        {
            set { _clientModel.ClientName = value; }
        }

        public string HgmText
        {
            get { return _clientModel.HgmText; }
            set { _clientModel.HgmText = value; NotifyPropertyChanged(); }
        }

        public string HGM_OpenL
        {
            get { return _clientModel.HGM_OpenL; }
            set { _clientModel.HGM_OpenL = value; NotifyPropertyChanged(); }
        }

        public string HGM_ClosedL
        {
            get { return _clientModel.HGM_ClosedL; }
            set { _clientModel.HGM_ClosedL = value; NotifyPropertyChanged(); }
        }

        public string MessageBoard
        {
            get { return _clientModel.MessageBoard; }
            set { _clientModel.MessageBoard = value; NotifyPropertyChanged(); }
        }

        public string Game_Chat
        {
            get { return _clientModel.Game_Chat; }
            set { _clientModel.Game_Chat = value; NotifyPropertyChanged(); }
        }

        public string UserList
        {
            get { return _clientModel.UserList; }
            set { _clientModel.UserList = value; NotifyPropertyChanged(); }
        }

        public string TTT_msg
        {
            get { return _clientModel.TTT_msg; }
            set { _clientModel.TTT_msg = value; NotifyPropertyChanged(); }
        }

        public BitmapImage TTT1
        {
            get { return char_to_image(_clientModel.TicTacToe["x0y0"]); }
        }

        public BitmapImage TTT2
        {
            get { return char_to_image(_clientModel.TicTacToe["x1y0"]); }
        }

        public BitmapImage TTT3
        {
            get { return char_to_image(_clientModel.TicTacToe["x2y0"]); }
        }

        public BitmapImage TTT4
        {
            get { return char_to_image(_clientModel.TicTacToe["x0y1"]); }
        }

        public BitmapImage TTT5
        {
            get { return char_to_image(_clientModel.TicTacToe["x1y1"]); }
        }

        public BitmapImage TTT6
        {
            get { return char_to_image(_clientModel.TicTacToe["x2y1"]); }
        }

        public BitmapImage TTT7
        {
            get { return char_to_image(_clientModel.TicTacToe["x0y2"]); }
        }

        public BitmapImage TTT8
        {
            get { return char_to_image(_clientModel.TicTacToe["x1y2"]); }
        }

        public BitmapImage TTT9
        {
            get { return char_to_image(_clientModel.TicTacToe["x2y2"]); }
        }

        public Visibility TTTBoard
        {
            get { return (_clientModel.TTTg ? Visibility.Visible : Visibility.Hidden); }
            set { _clientModel.TTTBoard = ((TTTBoard == Visibility.Visible) ? true : false); NotifyPropertyChanged(); }
        }

        public Visibility HMBoard
        {
            get { return (_clientModel.HMBoard ? Visibility.Visible : Visibility.Hidden); }
            set { _clientModel.HMBoard = ((HMBoard == Visibility.Visible) ? true : false); NotifyPropertyChanged(); }
        }

        public BitmapImage HMFigure
        {
            get { return _clientModel.HMFigure; }
            set { _clientModel.HMFigure = value; NotifyPropertyChanged(); }
        }


        public BitmapImage char_to_image(string k)
        {
            if (k == "x") { return ttt_x; }
            else if (k == "o") { return ttt_o; }
            else { return ttt_k; }
        }

        public DelegateCommand ConnectCommand { get; set; }
        public DelegateCommand SendCommand { get; set; }
        public DelegateCommand TTTMove_1 { get; }
        public DelegateCommand TTTMove_2 { get; }
        public DelegateCommand TTTMove_3 { get; }
        public DelegateCommand TTTMove_4 { get; }
        public DelegateCommand TTTMove_5 { get; }
        public DelegateCommand TTTMove_6 { get; }
        public DelegateCommand TTTMove_7 { get; }
        public DelegateCommand TTTMove_8 { get; }
        public DelegateCommand TTTMove_9 { get; }
        #endregion 

        #region Private and Internal Vars/Props
        private readonly ClientModel _clientModel;
        #endregion 

        /// <summary>
        /// Constructor creates the Model!
        /// </summary>
        public ClientViewModel()
        {
            //Create ourselves a model
            _clientModel = new ClientModel();
            //Subscribe to the Model's PropertyChanged event
            _clientModel.PropertyChanged += ClientModelChanged;
            //Create our chat command objects
            ConnectCommand = new DelegateCommand(
                a => _clientModel.Connect(),
                b => !_clientModel.Connected
            );
            SendCommand = new DelegateCommand(
                a => _clientModel.Send(),
                b => _clientModel.Connected
            );
            TTTMove_1 = new DelegateCommand(
                 a => _clientModel.TTT_Move("x0y0")
            );
            TTTMove_2 = new DelegateCommand(
                 a => _clientModel.TTT_Move("x1y0")
            );
            TTTMove_3 = new DelegateCommand(
                 a => _clientModel.TTT_Move("x2y0")
            );
            TTTMove_4 = new DelegateCommand(
                 a => _clientModel.TTT_Move("x0y1")
            );
            TTTMove_5 = new DelegateCommand(
                 a => _clientModel.TTT_Move("x1y1")
            );
            TTTMove_6 = new DelegateCommand(
                 a => _clientModel.TTT_Move("x2y1")
            );
            TTTMove_7 = new DelegateCommand(
                 a => _clientModel.TTT_Move("x0y2")
            );
            TTTMove_8 = new DelegateCommand(
                 a => _clientModel.TTT_Move("x1y2")
            );
            TTTMove_9 = new DelegateCommand(
                 a => _clientModel.TTT_Move("x2y2")
            );
            //Create our game challenge objects
        }


        #region Event Listeners
        private void ClientModelChanged(object sender, PropertyChangedEventArgs e)
        {
            // This our UI of any changes in the following values, without it, our UI is blind to change.
            if (e.PropertyName.Equals("Connected"))
            {
                NotifyPropertyChanged("Connected");

                // Command execute changes
                ConnectCommand.RaiseCanExecuteChanged();
                SendCommand.RaiseCanExecuteChanged();
            }
            else if (e.PropertyName.Equals("MessageBoard"))
            {
                NotifyPropertyChanged("MessageBoard");
            }
            else if (e.PropertyName.Equals("Game_Board"))
            {
                NotifyPropertyChanged("Game_Board");
            }
            else if (e.PropertyName.Equals("UserList"))
            {
                NotifyPropertyChanged("UserList");
            }
            else if (e.PropertyName.Equals("TTTg"))
            {
                NotifyPropertyChanged("TTTg");
                NotifyPropertyChanged("TTTBoard");

                /* Command execute changes
                TTTMove_1.RaiseCanExecuteChanged();
                TTTMove_2.RaiseCanExecuteChanged();
                TTTMove_3.RaiseCanExecuteChanged();
                TTTMove_4.RaiseCanExecuteChanged();
                TTTMove_5.RaiseCanExecuteChanged();
                TTTMove_6.RaiseCanExecuteChanged();
                TTTMove_7.RaiseCanExecuteChanged();
                TTTMove_8.RaiseCanExecuteChanged();
                TTTMove_9.RaiseCanExecuteChanged();
                */
            }
            else if (e.PropertyName.Equals("TTT_msg"))
            {
                NotifyPropertyChanged("TTT_msg");
            }
            else if (e.PropertyName.Equals("HMBoard"))
            {
                NotifyPropertyChanged("HMBoard");
            }
            else if (e.PropertyName.Equals("HMFigure"))
            {
                NotifyPropertyChanged("HMFigure");
            }
            else if (e.PropertyName.Equals("HgmText"))
            {
                NotifyPropertyChanged("HgmText");
            }
            else if (e.PropertyName.Equals("HGM_OpenL"))
            {
                NotifyPropertyChanged("HGM_OpenL");
            }
            else if (e.PropertyName.Equals("HGM_ClosedL"))
            {
                NotifyPropertyChanged("HGM_ClosedL");
            }
            else if (e.PropertyName.Equals("Message"))
            {
                NotifyPropertyChanged("Message");
            }
            else if (e.PropertyName.Equals("TTT1"))
            {
                NotifyPropertyChanged("TTT1");
            }
            else if (e.PropertyName.Equals("TTT2"))
            {
                NotifyPropertyChanged("TTT2");
            }
            else if (e.PropertyName.Equals("TTT3"))
            {
                NotifyPropertyChanged("TTT3");
            }
            else if (e.PropertyName.Equals("TTT4"))
            {
                NotifyPropertyChanged("TTT4");
            }
            else if (e.PropertyName.Equals("TTT5"))
            {
                NotifyPropertyChanged("TTT5");
            }
            else if (e.PropertyName.Equals("TTT6"))
            {
                NotifyPropertyChanged("TTT6");
            }
            else if (e.PropertyName.Equals("TTT7"))
            {
                NotifyPropertyChanged("TTT7");
            }
            else if (e.PropertyName.Equals("TTT8"))
            {
                NotifyPropertyChanged("TTT8");
            }
            else if (e.PropertyName.Equals("TTT9"))
            {
                NotifyPropertyChanged("TTT9");
            }
        }
        #endregion

        #region INPC Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string prop = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
        #endregion NPC Implementation
    }
}