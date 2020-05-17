
using GM_ChatLibrary;
using System;
using System.Net.Sockets;
using System.Threading;


namespace Gearmast_Server   
{
    /// <summary>
    /// This object exists to receive communications from the remote host associated with the socket
    /// </summary>
    public class HandleClient
    {
        //Socket associated with this remote host / client
        private TcpClient _clientSocket;
        //Name/number of the client
        private string _clientName;

        /// <summary>
        /// Initializes the object and starts the new thread.
        /// </summary>
        /// <param name="clientSocket">TcpClient to talk with client</param>
        /// <param name="clientName">Name of client</param>
        public void StartClient(TcpClient clientSocket, string clientName)
        {
            _clientName = clientName;
            _clientSocket = clientSocket;
            var thread = new Thread(DoChat);
            Program.ThreadList.Add(clientName, thread); // adding our live chat handling thread to our ThreadList of our server
            thread.Start();
        }

        /// <summary>
        /// Executed on a new thread - services messages sent by remote host/client
        /// </summary>
        private void DoChat()
        {
            while (true)
            {
                try
                {
                    string dataFromClient = _clientSocket.ReadString();
                    if ((dataFromClient[0] == '#') && (dataFromClient[1] == '(')) // lobby chat message
                    {
                        string msg = dataFromClient.Substring(2);
                        Program.Broadcast(msg, _clientName, true);
                        Console.WriteLine("[" + DateTime.Now + "] (" + _clientName + "): " + msg);
                    }
                    else if ((dataFromClient[0] == 'p') && (dataFromClient[1] == '(')) // game chat message
                    {
                        //string msg = dataFromClient.Substring(2);
                        //Program.Broadcast(msg, _clientName, true, "p");
                    }
                    else if ((dataFromClient[0] == 'l') && (dataFromClient[1] == '(')) // userlist request
                    {
                        Program.UserList_Update(false, _clientName);
                        //Console.WriteLine("[" + DateTime.Now + "] (" + _clientName + ") requested a fresh copy of the user list.");
                    }
                    else if ((dataFromClient[0] == 'h') && (dataFromClient[1] == '(')) // hangman request
                    {
                        // h(p2(word
                        string[] msgspl = (dataFromClient.Substring(2)).Split('(');
                        Program.Start_HGM(_clientSocket, _clientName, msgspl[0], msgspl[1]);
                    }
                    else if ((dataFromClient[0] == 't') && (dataFromClient[1] == '(')) // tictactoe request
                    {
                        Program.Start_TTT(_clientSocket, _clientName, dataFromClient.Substring(2));
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}