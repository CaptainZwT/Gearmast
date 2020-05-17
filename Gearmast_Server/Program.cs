using GM_ChatLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Media.Imaging;

namespace Gearmast_Server
{
    class Program
    {
        //This is a dictionary of key,value pairs used to store the names and sockets of each client.
        public static Dictionary<string, TcpClient> ClientList = new Dictionary<string, TcpClient>();
        public static Dictionary<string, Thread> ThreadList = new Dictionary<string, Thread>();
        private static bool alive = true;
        /// <summary>
        /// Listens for new connections and handles them.
        /// </summary>
        static void Main()
        {
            var serverSocket = new TcpListener(IPAddress.Any, 8888);
            serverSocket.Start();
            Console.WriteLine("[" + DateTime.Now + "] (Server) " + "Gearmast network started...");

            //Set a thread running for our server commands
            new Thread(GetCommand).Start();
            //Set a thread running for users leaving
            //new Thread(UserCheck).Start();

            while (alive) // Main thread running for users connecting
            {
                //This next line of code actually blocks
                var clientSocket = serverSocket.AcceptTcpClient();
                //Somebody connected and sent us data... and no clientSocket doesn't have a method called ReadString: See TcpClientExtension.cs
                string dataFromClient = clientSocket.ReadString();
                //Add the name and StringSocket to the Dictionary object
                string nm = dataFromClient.Substring(2);
                ClientList.Add(nm, clientSocket);
                //Tell everyone that someone new joined!
                Broadcast(" [" + DateTime.Now + "] (Server) " + nm + " has joined the network.", dataFromClient, false);
                //Create a new object to Handle all future incoming messages from this client
                var client = new HandleClient();
                //Start that thread running
                client.StartClient(clientSocket, nm);
                //Log the fact to the server console
                Console.WriteLine("[" + DateTime.Now + "] (Server) " + nm + " has joined the network.");
                UserList_Update();
            }
        }

        /// <summary>
        /// Executed as a new thread on server boot, it handles server commands from the main command prompt
        /// </summary>

        private static void GetCommand()
        {
            while (true)
            {
                string inp = Console.ReadLine();
                if ((inp == "/?") || (inp == "/help"))
                {
                    Console.WriteLine("List of Commands:");
                    Console.WriteLine("    /say         : broadcasts a message from the server to clients.");
                    Console.WriteLine("    /kick         : removes a client, and ends their ability to interact.");
                    Console.WriteLine("    /users online: tells you how many active users are on the server.");
                }
                else if (inp.Substring(0, 5) == "/stop")
                {
                    Broadcast("[" + DateTime.Now + "] (Server): Gearmast Gaming Network is shutting down, be back later. ", "", false);
                    foreach (var item in ClientList.ToList())
                    {
                        ThreadList[item.Key].Abort();
                        ClientList.Remove(item.Key);
                    }
                    alive = false;
                    Console.WriteLine("[" + DateTime.Now + "] Server shut down.");
                }
                else if (inp.Substring(0, 5) == "/kick")
                {
                    if (ClientList.ContainsKey(inp.Substring(6)))
                    {
                        ClientList[inp.Substring(6)].WriteString("#( [" + DateTime.Now + "](Server): You have been kicked from the server");
                        Console.WriteLine("[" + DateTime.Now + "] (Server): " + inp.Substring(6) + " has been kicked from the server.");
                        ThreadList[inp.Substring(6)].Abort();
                        ClientList.Remove(inp.Substring(6));
                        UserList_Update();
                        Broadcast("[" + DateTime.Now + "] (Server): " + inp.Substring(6) + " has been kicked from the server.", "", false);
                    }
                    else
                    {
                        Console.WriteLine(inp.Substring(6) + " does not exist, to be kicked.");
                    }
                }
                else if ((inp.Length > 5) && (inp.Substring(0, 4) == "/say"))
                {
                    Broadcast(inp.Substring(4), "Server", true);
                    Console.WriteLine("[" + DateTime.Now + "] (Server):" + inp.Substring(5));
                }
                else if ((inp.Length > 8) && (inp.Substring(0, 6) == "/users"))
                {
                    string[] msgsplit = inp.Substring(7).Split(' ');
                    //Console.WriteLine("(" + inp.Substring(6) + ")(" + msgsplit[0] + ")");
                    if (msgsplit[0] == "online")
                    {
                        string list = "";
                        int i = 1;
                        foreach (var item in ClientList)
                        {
                            list += (item.Key + ((i < ClientList.Count) ? ", " : ""));
                            i++;
                        }
                        Console.WriteLine("[" + DateTime.Now + "] (Server) " + list);
                        Console.WriteLine("[" + DateTime.Now + "] (Server) " + ClientList.Count + " user(s) online.");
                    }
                }
                else
                {
                    Console.WriteLine("That command was not understood, please refer to /? for list of valid commands.");
                }
            }
        }

        /// <summary>
        /// Executed as a new thread on server boot, it handles finding out if clients disconnect
        /// </summary>

        private static void UserCheck()
        {
            //while (true)
            //{
            //    foreach (var item in ClientList.ToList())
            //    {
            //        if (!item.Value.Connected)
            //        {
            //            Console.WriteLine("[" + DateTime.Now + "] (Server) " + item.Key + " has left the network.");
            //            Broadcast(" [" + DateTime.Now + "] (Server) " + item.Key + " has left the network.", item.Key, false);
            //            ClientList.Remove(item.Key);
            //            UserList_Update();
            //        }
            //    }
            //}
        }

        // Text Updates

        /// <summary>
        /// A simple broadcast message function that resides here to allow the clients to broadcast
        /// incomming messages to everyone. 
        /// </summary>
        /// <param name="msg">The message to broadcast</param>
        /// <param name="uname">The user's name who sent it</param>
        /// <param name="flag"></param>
        /// 


        public static void Broadcast(string msg, string uname, bool flag, string sym = "#")
        {
            foreach (var item in ClientList)
            {
                var broadcastSocket = item.Value;
                var m = flag ? sym + "( [" + DateTime.Now + "] (" + uname + "): " + msg : sym + "(" + msg;
                broadcastSocket.WriteString(m);
            }
        }

        /// <summary>
        /// A simple broadcast message function that takes parameters on either, sending a current userlist to all or
        /// sending a current userlist to a specific user
        /// </summary>
        /// <param name="everyone">Update everyone or not</param>
        /// <param name="uname">If not, who are we sending it to</param>
        /// 

        public static void UserList_Update(bool everyone = true, string name = "")
        {
            if (everyone)
            {
                string Userlist_msg = "Connected Users: \r\n";
                foreach (var item in ClientList)
                {
                    Userlist_msg += (" " + item.Key + " \n");
                }
                foreach (var item in ClientList)
                {
                    var broadcastSocket = item.Value;
                    broadcastSocket.WriteString("$(" + Userlist_msg);
                }
            }
            else
            {
                string Userlist_msg = "Connected Users: \r\n";
                foreach (var item in ClientList)
                {
                    Userlist_msg += (" " + item.Key + " \n");
                }
                var broadcastSocket = ClientList.FirstOrDefault(x => x.Key == name).Value;
                broadcastSocket.WriteString("$(" + Userlist_msg);
            }
        }

        // Game related functions

        public static void Start_TTT(TcpClient p1socket, string player1, string player2)
        {

            Dictionary<string, char> TTTGrid = new Dictionary<string, char>()
            {
                { "x0y0", 'k'},
                { "x1y0", 'k'},
                { "x2y0", 'k'},
                { "x0y1", 'k'},
                { "x1y1", 'k'},
                { "x2y1", 'k'},
                { "x0y2", 'k'},
                { "x1y2", 'k'},
                { "x2y2", 'k'}
            };
            TcpClient p2socket = ClientList[player2];
            string TTTurn = player1;
            bool tg = true;
            //
            //  g(ttt(c = TicTacToe Setup
            //  g(ttt() = TicTacToe Update
            //  g(ttt(x = TicTacToe clean
            //  g)ttt)x0y0  = TicTacToe move
            p1socket.WriteString("g(ttt(c");
            p2socket.WriteString("g(ttt(c");
            while (tg)
            {
                TcpClient socket = ClientList[TTTurn];
                string inp = socket.ReadString();
                if (inp.Substring(0, 6) == "g)ttt)")
                {
                    string pos = inp.Substring(6, 4);
                    char ch = ((TTTurn == player1) ? 'x' : 'o');
                    TTTGrid[pos] = ch;
                    p1socket.WriteString("g(ttt()" + pos + ")" + ch);
                    p2socket.WriteString("g(ttt()" + pos + ")" + ch);
                    TTTurn = ((TTTurn == player1) ? player2 : player1);
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        // check horizontal wins
                        if ((TTTGrid["x" + i + "y0"] != 'k') && (TTTGrid["x" + i + "y0"] == TTTGrid["x" + i + "y1"]) && (TTTGrid["x" + i + "y1"] == TTTGrid["x" + i + "y2"])) {
                            tg = false;
                            string winner = (TTTGrid["x" + i + "y0"] == 'x') ? player1 : player2;
                            string loser = (winner == player1) ? player2 : player1;
                            string gamedec = winner + " lost to " + loser + " in a game of TicTacToe.";
                            Console.WriteLine(gamedec);
                            Broadcast(gamedec, "Server", true);
                        }
                        // check vertical wins
                        else if ((TTTGrid["x0y" + i] != 'k') && (TTTGrid["x0y" + i] == TTTGrid["x1y" + i]) && (TTTGrid["x1y" + i] == TTTGrid["x2y" + i]))
                        {
                            tg = false;
                            string winner = (TTTGrid["x0y" + i] == 'x') ? player1 : player2;
                            string loser = (winner == player1) ? player2 : player1;
                            string gamedec = winner + " lost to " + loser + " in a game of TicTacToe.";
                            Console.WriteLine(gamedec);
                            Broadcast(gamedec, "Server", true);
                        }
                    }
                    if (tg) // protecting it, incase we don't need it // diagonal wins
                    {
                        if ((TTTGrid["x0y0"] != 'k') && (TTTGrid["x0y0"] == TTTGrid["x1y1"]) && (TTTGrid["x1y1"] == TTTGrid["x2y2"])){
                            tg = false;
                            string winner = (TTTGrid["x0y0"] == 'x') ? player1 : player2;
                            string loser = (winner == player1) ? player2 : player1;
                            string gamedec = winner + " lost to " + loser + " in a game of TicTacToe.";
                            Console.WriteLine(gamedec);
                            Broadcast(gamedec, "Server", true);
                        }
                        else if ((TTTGrid["x2y0"] != 'k') && (TTTGrid["x2y0"] == TTTGrid["x1y1"]) && (TTTGrid["x1y1"] == TTTGrid["x0y2"])){
                            tg = false;
                            string winner = (TTTGrid["x2y0"] == 'x') ? player1 : player2;
                            string loser = (winner == player1) ? player2 : player1;
                            string gamedec = winner + " lost to " + loser + " in a game of TicTacToe.";
                            Console.WriteLine(gamedec);
                            Broadcast(gamedec, "Server", true);
                        }
                    }
                }
            p1socket.WriteString("g(ttt(x");
            p2socket.WriteString("g(ttt(x");
            }
        }


        public static void Start_HGM(TcpClient p1socket, string player1, string player2, string word)
        {
            TcpClient p2socket = ClientList[player2];
            int lostlives = 0;
            string at_ltrs = " ", cur_word = "";
            bool tg = true;

            // initial word creation
            for (int i = 0; i < word.Length; i++)
            {
                if (at_ltrs.Contains(word[i]))
                {
                    cur_word += (" " + word[i]);
                }
                else
                {
                    cur_word += " _";
                }
            }
            // Update HGM game
            p1socket.WriteString("g(hgm)" + cur_word + ")" + at_ltrs + ")" + lostlives);
            p2socket.WriteString("g(hgm)" + cur_word + ")" + at_ltrs + ")" + lostlives);
            //
            Console.WriteLine("[" + DateTime.Now + "] New Hangman game started between " + player1 + " and " + player2 + ".");
            p1socket.WriteString("p)Server(  You start a game of Hangman against " + player2 + ".  The word is " + word);
            p2socket.WriteString("p)Server(  You start a game of Hangman against " + player1 + ".");

            while (tg)
            {
                string Inp = p2socket.ReadString();
                Console.WriteLine(Inp);
                if ((Inp.Substring(0, 6) == "g)hgm)") && ((!at_ltrs.Contains(char.ToUpper(Inp[6])))))
                {
                    char ul = char.ToUpper(Inp[6]);
                    char ll = char.ToLower(Inp[6]);
                    at_ltrs += ul;
                    cur_word = "";
                    for (int i = 0; i < word.Length; i++)
                    {
                        if (at_ltrs.Contains(char.ToUpper(word[i])))
                        {
                            cur_word += (" " + char.ToUpper(word[i]));
                        }
                        else
                        {
                            cur_word += " _";
                        }
                    }

                    if (!cur_word.Contains('_'))
                    {
                        tg = false;
                        string gamedec = player2 + " beat " + player1 + " in a game of Hangman with " + (6 - lostlives) + " remaining lives.";
                        Console.WriteLine(gamedec);
                        Broadcast(gamedec, "Server", true);
                    }


                    if ((!word.Contains(ll)) && (!word.Contains(ul)))
                    {
                        lostlives++;
                        p2socket.WriteString("p)Server( You missed the letter " + Inp[6] + ". " + lostlives + " lives lost.");
                        if (lostlives == 6)
                        {
                            p1socket.WriteString("g)");
                            p2socket.WriteString("g)");
                            tg = false;
                            string gamedec = player2 + " lost to " + player1 + " in a game of Hangman.";
                            Console.WriteLine(gamedec);
                            Broadcast(gamedec, "Server", true);
                        }
                    }
                    else
                    {
                        p2socket.WriteString("p)Server( You hit the letter " + Inp[6] + ". " + lostlives + " lives lost.");
                    }

                    // Update HGM game
                    p1socket.WriteString("g(hgm)" + cur_word + ")" + at_ltrs + ")" + lostlives);
                    p2socket.WriteString("g(hgm)" + cur_word + ")" + at_ltrs + ")" + lostlives);
                    //
                }
                else if (Inp.Substring(0, 6) == "g)hgm)")
                {
                    p2socket.WriteString("p)Server( You've already tried that letter, or it was a invalid letter.");
                }
            }
        }
    }
}