using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Data.SQLite;
using System.IO;
using System.Diagnostics;

namespace ChatRoom
{
    class Program
    {
        static void Main(string[] args)
        {
            short port = 8000;
            int maxConnections = 100;
            int maxMemory = -1;

            try
            {
                switch (args.Length)
                {
                    case 1:
                        port = Convert.ToInt16(args[0]);
                        break;
                    case 2:
                        port = Convert.ToInt16(args[0]);
                        maxConnections = Convert.ToInt32(args[1]);
                        break;
                    case 3:
                        port = Convert.ToInt16(args[0]);
                        maxConnections = Convert.ToInt32(args[1]);
                        maxMemory = Convert.ToInt32(args[2]);
                        break;
                    default:
                        break;
                } 
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid argument format(s). Usage: ChatRoomServer.exe <port> <max connections> <max memory (megabytes)>");
            }

            try
            {
                ChatRoomServer server = new ChatRoomServer();
                Console.CancelKeyPress += delegate { server.Close(); Environment.Exit(0); };
                server.Start(port, maxConnections, maxMemory);
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine("OutOfMemoryException Exception (try lower the max connections)");
                Environment.Exit(-1);
            }
        }
    }

    public class ChatRoomServer
    {
        private Socket socket;
        private ClientConnection[] clientConnections;
        private UserHandler userHandler;
        private List<ChatRoom> chatRooms;
        private string roomIdCharacters = "abcdef1234567890";
        private Random random;

        public ChatRoomServer()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            chatRooms = new List<ChatRoom>();
            random = new Random();
            for (int i = 0; i < 10; i++) chatRooms.Add(new ChatRoom() { ID = GetRoomId(), Name = string.Format("Public Room " + i), MaxUsers = 10, Password = "", TempRoom = false });
        }

        public void Start(int port, int maxConnections, int maxMemory = -1)
        {
            Console.WriteLine("{0} - Starting ChatRoomServer on port {1} for {2} clients...", ChatTime.GetTimeFormat(), port, maxConnections);
            userHandler = new UserHandler("users.db", maxConnections);
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            socket.Listen(5);
            clientConnections = new ClientConnection[maxConnections];
            Process process;
            for (int i = 0; i < maxConnections; i++)
            {
                int tempI = i;
                new Thread(new ParameterizedThreadStart(delegate {  ClientHandler(tempI); })).Start();
                if (i % 2000 == 0)
                {
                    process = Process.GetCurrentProcess();
                    if (maxMemory != -1 && (process.PrivateMemorySize64 / 2000000) > maxMemory)
                    {
                        Console.WriteLine("Exceeded memory limit at {0} megabytes at client number {1}.", maxMemory, i + 1);
                        break;
                    }
                }
            }
            Console.WriteLine("{0} - Done!", ChatTime.GetTimeFormat());
        }

        private void ClientHandler(int id)
        {
            while (true)
            {
                ClientConnection currentClient = clientConnections[id] = new ClientConnection(id, socket.Accept());
                try
                {
                    Console.WriteLine("{0} <{1}> - Received connection from {2}", ChatTime.GetTimeFormat(), currentClient.ID, currentClient.GetConnectionString());
                    byte[] receiveBuffer = new byte[1024 * 2];
                    int receiveSize = currentClient.Connection.Receive(receiveBuffer);
                    LoginExchange loginExchange = LoginExchange.Decode(receiveBuffer, receiveSize);
                    Console.WriteLine("{0} <{1}> - Login request from '{2}'.", ChatTime.GetTimeFormat(), currentClient.ID, loginExchange.Username);
                    LoginExchangeResponse loginExchangeResponse = userHandler.HandleLoginExchange(loginExchange, currentClient);

                    string loginResponseData = loginExchangeResponse.Encode();
                    byte[] loginResponse = Encoding.UTF8.GetBytes(loginResponseData);
                    currentClient.Connection.Send(loginResponse);

                    if (loginExchangeResponse.ExchangeType == LoginExchangeResponseCode.Successful)
                    {
                        User user = userHandler.GetUser(id);
                        Console.WriteLine("{0} <{1}> - User '{2}' logged in with token '{3}'.", ChatTime.GetTimeFormat(), currentClient.ID, user.Name, user.Token);
                        while (user != null)
                        {
                            receiveBuffer = new byte[1024 * 2];
                            receiveSize = currentClient.Connection.Receive(receiveBuffer);
                            if (receiveSize > 0)
                            {
                                ChatExchange chatExchange = ChatExchange.Decode(Encoding.UTF8.GetString(receiveBuffer, 0, receiveSize));
                                if (user.Verify(chatExchange))
                                {
                                    switch (chatExchange.ExchangeType)
                                    {
                                        case ChatExchangeType.Message:
                                            Console.WriteLine("{0} <{1}> - [{2}] {3}: {4}", ChatTime.GetTimeFormat(), currentClient.ID, user.CurrentChatRoom.Name, user.Name, chatExchange.Exchange);
                                            MessageHandler(chatExchange, user);
                                            break;
                                        case ChatExchangeType.Command:
                                            Console.WriteLine("{0} <{1}> - Command: {2}", ChatTime.GetTimeFormat(), currentClient.ID, chatExchange.Exchange);
                                            CommandHandler(chatExchange, user);
                                            break;
                                        case ChatExchangeType.ChatRoomList:
                                            Console.WriteLine("{0} <{1}> - Requested Chat Room List", ChatTime.GetTimeFormat(), currentClient.ID);
                                            ChatRoomListHandler(chatExchange, user);
                                            break;
                                        case ChatExchangeType.JoinRoom:
                                            Console.WriteLine("{0} <{1}> - Join Room: ID '{2}'", ChatTime.GetTimeFormat(), currentClient.ID, chatExchange.ExchangeTargetName);
                                            JoinRoomHandler(chatExchange, user);
                                            break;
                                        case ChatExchangeType.LeaveRoom:
                                            Console.WriteLine("{0} <{1}> - Leave Room", ChatTime.GetTimeFormat(), currentClient.ID);
                                            LeaveRoomHandler(chatExchange, user);
                                            break;
                                        case ChatExchangeType.CreateRoom:
                                            Console.WriteLine("{0} <{1}> - Create Room: '{2}'", ChatTime.GetTimeFormat(), currentClient.ID, chatExchange.ExchangeTargetName);
                                            CreateChatRoomHandler(chatExchange, user);
                                            break;
                                        case ChatExchangeType.Error:
                                            Console.WriteLine("{0} <{1}> - ChatExchange Error Occurred", ChatTime.GetTimeFormat(), currentClient.ID, chatExchange.ExchangeTargetName);
                                            break;
                                    }
                                }
                            }
                            else break;
                        }
                    }
                    Console.WriteLine("{0} <{1}> - Ending connection.", ChatTime.GetTimeFormat(), currentClient.ID);
                }
                catch (SocketException)
                {
                    Console.WriteLine("{0} <{1}> - Lost connection.", ChatTime.GetTimeFormat(), currentClient.ID);
                }
                finally
                {
                    userHandler.Logout(id);
                    currentClient = null;
                }
            }
        }

        public void JoinRoomHandler(ChatExchange chatExchange, User user)
        {
            ChatRoom requestedRoom = GetRoom(chatExchange.ExchangeTargetName);
            string responseExchange = String.Empty;
            if (!requestedRoom.RequiresPassword)
            {
                requestedRoom.AddUser(user);
                responseExchange = "JOINED";
            }
            else
            {
                if (chatExchange.Exchange == requestedRoom.Password)
                {
                    requestedRoom.AddUser(user);
                    responseExchange = "JOINED";
                }
                else responseExchange = "INVALIDPASSWORD";
            }
            string encoded = ChatExchange.Encode("", ChatExchangeType.Command, ChatExchangeTarget.Individual, "Server", "", responseExchange);
            byte[] sendBuffer = Encoding.UTF8.GetBytes(encoded);
            user.Connection.Connection.Send(sendBuffer);
        }

        public void CreateChatRoomHandler(ChatExchange chatExchange, User user)
        {
            string roomName = chatExchange.ExchangeTargetName;
            string roomPassword = chatExchange.Exchange;

            ChatRoom newRoom = new ChatRoom { ID = GetRoomId(), MaxUsers = 10, Name = roomName, Password = roomPassword, TempRoom = true };
            chatRooms.Add(newRoom);

            newRoom.AddUser(user);
            user.IsRoomHost = true;

            string encoded = ChatExchange.Encode("", ChatExchangeType.Command, ChatExchangeTarget.Individual, "Server", newRoom.ID, "JOINED");
            byte[] sendBuffer = Encoding.UTF8.GetBytes(encoded);
            user.Connection.Connection.Send(sendBuffer);
        }

        public void LeaveRoomHandler(ChatExchange chatExchange, User user)
        {
            user.LeaveRoom();
            string encoded = ChatExchange.Encode("", ChatExchangeType.Command, ChatExchangeTarget.Individual, "Server", "", "LEFT");
            byte[] sendBuffer = Encoding.UTF8.GetBytes(encoded);
            user.Connection.Connection.Send(sendBuffer);
        }

        public ChatRoom GetRoom(string id)
        {
            foreach (ChatRoom room in chatRooms) if (room.ID == id) return room;
            return null;
        }

        public void ChatRoomListHandler(ChatExchange chatExchange, User user)
        {
            RemoveEmptyTempRooms();
            List<AvailableRoom> availableRooms = new List<AvailableRoom>();
            foreach (ChatRoom room in chatRooms) availableRooms.Add(room.GetAvailableRoom());
            string encoded = RoomListExchange.Encode(availableRooms);
            byte[] sendBuffer = Encoding.UTF8.GetBytes(encoded);
            user.Connection.Connection.Send(sendBuffer);
        }

        public void RemoveEmptyTempRooms()
        {
            for (int i = 0; i < chatRooms.Count; i++)
            {
                if (!chatRooms[i].IsActive)
                {
                    foreach (User user in userHandler.Users) if (user != null && user.InRoom && user.CurrentChatRoom.Equals(chatRooms[i])) user.CurrentChatRoom = null;
                    chatRooms.Remove(chatRooms[i]);
                }
            }
        }

        private string GetRoomId()
        {
            string token = String.Empty;
            for (int i = 0; i < 10; i++) token += roomIdCharacters[random.Next(roomIdCharacters.Length)];
            return token;
        }

        public void MessageHandler(ChatExchange chatExchange, User user)
        {
            switch (chatExchange.ExchangeTarget)
            {
                case ChatExchangeTarget.ChatRoom:
                    user.CurrentChatRoom.Message(chatExchange);
                    break;
                case ChatExchangeTarget.Individual:
                    break;
                case ChatExchangeTarget.Server:
                default:
                    break;
            }
        }

        public void CommandHandler(ChatExchange chatExchange, User user)
        {
            string command = chatExchange.Exchange;
            string[] parts = command.Split(' ');
            switch (parts[0].ToLower())
            {
                case "help":
                    string helpMessage = string.Format("Available Commands:{0}> '/help' - Display a list of commands.{0}> /poke <user> [message] - Poke a user with a message.", Environment.NewLine);
                    user.Connection.Connection.Send(new CommandExchangeResponse() { ResponseType = CommandResponse.Success, OutputMessage = helpMessage, FurtherInfo = "Displays a list of commands." }.EncodeBytes());
                    break;
                case "poke":
                    string response = string.Empty;
                    CommandResponse responseType = CommandResponse.Fail;
                    if (command.Length > "poke".Length)
                    {
                        string username = parts[1];
                        User targetUser = userHandler.GetUserByName(username);
                        if (targetUser != null)
                        {
                            targetUser.Connection.Connection.Send(new ChatExchange() { SenderName = user.Name, ExchangeTarget = ChatExchangeTarget.Individual, ExchangeType = ChatExchangeType.Command, ExchangeTargetName = "poke", Exchange = parts.Length >= 3 ? string.Format("{0}: {1}", user.Name, command.Substring((parts[0] + parts[1]).Length + 1, command.Length - (parts[0] + parts[1]).Length - 1)) : "You were poked." });
                            responseType = CommandResponse.Success;
                            response = string.Format("Poked '{0}'...", username);
                        }
                        else response = string.Format("Could not find user '{0}'...", username);
                    }
                    else response = "Target user required. > /poke <user> [message]";
                    user.Connection.Connection.Send(new CommandExchangeResponse() { ResponseType = responseType, OutputMessage = response, FurtherInfo = "Pokes a user." }.EncodeBytes());
                    break;
                default:
                    user.Connection.Connection.Send(new CommandExchangeResponse() { ResponseType = CommandResponse.UnknownCommand, OutputMessage = "Unknown command.", FurtherInfo = "Command not found." }.EncodeBytes());
                    break;
            }
        }

        public void Close()
        {
            socket.Close();
        }
    }

    public class ClientConnection
    {
        public int ID { get; private set; }
        public Socket Connection { get; private set; }
        public bool Connected { get { return Connection.Connected; } }

        public ClientConnection(int ID, Socket Connection)
        {
            this.ID = ID;
            this.Connection = Connection;
        }

        public string GetConnectionString()
        {
            IPEndPoint remoteEndPoint = Connection.RemoteEndPoint as IPEndPoint;
            return string.Format("{0}:{1}", remoteEndPoint.Address, remoteEndPoint.Port);
        }

        public override string ToString()
        {
            return string.Format("ClientConnection({0}, '{1}')", ID, GetConnectionString());
        }

        public void Disconnect()
        {
            if (Connected) Connection.Close();
        }
    }

    public class UserHandler
    {
        private SQLiteConnection dbConnection;
        private const string tokenCharacters = "abcdef1234567890";

        public User[] Users { get; private set; }

        public UserHandler(string userDatabaseName, int maxConnections)
        {
            if (!File.Exists(userDatabaseName)) SQLiteConnection.CreateFile(userDatabaseName);
            dbConnection = new SQLiteConnection(string.Format("Data Source={0};Version=3", userDatabaseName));
            dbConnection.Open();
            SQLiteCommand command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS users (username VARCHAR(20), password VARCHAR(64))", dbConnection);
            command.ExecuteNonQuery();
            Users = new User[maxConnections];
        }

        public LoginExchangeResponse HandleLoginExchange(LoginExchange exchange, ClientConnection connection)
        {
            switch (exchange.ExchangeType)
            {
                case LoginExchangeType.Login:
                    return HandleLogin(exchange, connection);
                case LoginExchangeType.Register:
                    return HandleRegister(exchange, connection);
                case LoginExchangeType.Logout:
                    HandleLogout(exchange);
                    return null;
                default:
                    return null;
            }
        }

        public void SendToAll(string message)
        {
            ChatExchange e = new ChatExchange { Exchange = message, ExchangeTarget = ChatExchangeTarget.Server,  };
        }

        public User GetUserByName(string name)
        {
            foreach (User user in Users) if (user != null && user.Name == name) return user;
            return null;
        }

        public User GetUser(int ID)
        {
            foreach (User user in Users) if (user != null && user.Connection.ID == ID) return user;
            return null;
        }

        public User GetUser(string loginToken)
        {
            foreach (User user in Users) if (user != null && user.Verify(loginToken)) return user;
            return null;
        }

        public void HandleLogout(LoginExchange exchange)
        {
            for (int i = 0; i < Users.Length; i++) if (Users[i] != null && Users[i].Token == exchange.LoginToken)
                {
                    Users[i].Logout();
                    Users[i] = null;
                }
        }

        public void Logout(int id)
        {
            for (int i = 0; i < Users.Length; i++) if (Users[i] != null && Users[i].Connection.ID == id)
                {
                    Users[i].Logout();
                    Users[i] = null;
                }
        }

        public LoginExchangeResponse HandleLogin(LoginExchange exchange, ClientConnection connection)
        {
            string username = exchange.Username;
            string password = exchange.Password;
            LoginExchangeResponseCode responseCode;
            string loginToken;
            if (IsLoggedIn(username))
            {
                responseCode = LoginExchangeResponseCode.AlreadyLoggedIn;
                loginToken = "";
            }
            else if (IsValidUser(username, password))
            {
                int reservedSlot = NextFreeSlot;
                if (reservedSlot >= 0)
                {
                    responseCode = LoginExchangeResponseCode.Successful;
                    loginToken = NewToken();
                    bool isAdmin = username == "Grimm" || username == "MaxG";
                    Users[reservedSlot] = new User() { Name = username, Connection = connection, Token = loginToken, IsAdmin = isAdmin };
                }
                else
                {
                    responseCode = LoginExchangeResponseCode.ServerFull;
                    loginToken = "";
                }
            }
            else
            {
                responseCode = LoginExchangeResponseCode.InvalidUsernameOrPassword;
                loginToken = "";
            }
            return new LoginExchangeResponse() { ExchangeType = responseCode, LoginToken = loginToken };
        }

        public LoginExchangeResponse HandleRegister(LoginExchange exchange, ClientConnection connection)
        {
            string username = exchange.Username;
            string password = exchange.Password;
            LoginExchangeResponseCode responseCode;
            string loginToken;
            if (IsValidUser(username, password))
            {
                responseCode = LoginExchangeResponseCode.UsernameExists;
                loginToken = "";
            }
            else
            {
                int reservedSlot = NextFreeSlot;
                if (reservedSlot >= 0)
                {
                    RegisterUser(username, password);
                responseCode = LoginExchangeResponseCode.Successful;
                loginToken = NewToken();
                User user = new User() { Name = username, Connection = connection, Token = loginToken };
                Users[reservedSlot] = user;
                }
                else
                {
                    responseCode = LoginExchangeResponseCode.ServerFull;
                    loginToken = "";
                }
            }
            return new LoginExchangeResponse() { ExchangeType = responseCode, LoginToken = loginToken };
        }

        public int NextFreeSlot
        {
            get
            {
                for (int i = 0; i < Users.Length; i++) if (Users[i] == null) return i;
                return -1;
            }
        }

        public void RegisterUser(string username, string password)
        {
            string sql = string.Format("INSERT INTO users (username, password) values ('{0}', '{1}')", username, password);
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();
        }

        public bool IsValidUser(string username, string password)
        {
            SQLiteCommand command = new SQLiteCommand(string.Format("SELECT * FROM users WHERE username = '{0}' AND password = '{1}'", username, password), dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            return reader.HasRows;
        }

        public bool IsLoggedIn(string username)
        {
            foreach (User user in Users) if (user != null && user.Name == username) return true;
            return false;
        }

        private static string NewToken()
        {
            Random random = new Random();
            string token = String.Empty;
            for (int i = 0; i < 10; i++) token += tokenCharacters[random.Next(tokenCharacters.Length)];
            return token;
        }
    }

    public class User
    {
        public string Token { get; set; }
        public string Name { get; set; }
        public ChatRoom CurrentChatRoom { get; set; }
        public ClientConnection Connection { get; set; }
        public bool InRoom { get { return CurrentChatRoom != null; } set { CurrentChatRoom = value ? CurrentChatRoom : null; } }
        public bool IsRoomHost { get; set; }
        public bool IsAdmin { get; set; }

        public void Logout()
        {
            Connection.Disconnect();
            Connection = null;
            LeaveRoom();
        }

        public override string ToString()
        {
            return string.Format("User('{0}' ({1}), {2})", Name, Token, Connection);
        }

        public bool Verify(ChatExchange chatExchange)
        {
            return Token == chatExchange.LoginToken;
        }

        public void LeaveRoom()
        {
            IsRoomHost = false;
            if (InRoom) CurrentChatRoom.Leave(this);
        }

        public bool Verify(string loginToken)
        {
            return Token == loginToken;
        }

        public void Message(ChatExchange exchange)
        {
            Connection.Connection.Send(Encoding.UTF8.GetBytes(exchange.Encode()));
        }
    }

    public class ChatRoom
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public List<User> Users { get; private set; }
        public int MaxUsers { get; set; }
        public int UserCount { get { return Users.Count; } }
        public bool RequiresPassword { get { return Password != ""; } }
        public string Password { get; set; }
        public bool TempRoom { get; set; }
        public bool IsActive { get { return !(TempRoom && UserCount == 0); } }

        public ChatRoom()
        {
            Users = new List<User>();
        }

        public bool AddUser(User user)
        {
            if (Users.Count < MaxUsers)
            {
                Users.Add(user);
                user.CurrentChatRoom = this;
                return true;
            }
            return false;
        }

        public void Leave(User user)
        {
            Users.Remove(user);
        }

        public void Message(ChatExchange exchange)
        {
            string userName = string.Empty;
            foreach (User user in Users) if (user.Token == exchange.LoginToken)
                {
                    exchange.SenderName = string.Format("{0}{1}", user.IsAdmin ? "<Admin> " : "", user.IsRoomHost ? string.Format("<Host> [{0}]", user.Name) : string.Format("[{0}]", user.Name));
                    foreach (User currentUser in Users) currentUser.Message(exchange);
                }
        }

        public bool HasUser(string logintoken)
        {
            foreach (User user in Users) if (user.Token == logintoken) return true;
            return false;
        }

        public AvailableRoom GetAvailableRoom()
        {
            return new AvailableRoom() { ID = this.ID, UserCount = this.Users.Count, MaxUsers = this.MaxUsers, Name = this.Name, RequiresPassword = this.RequiresPassword };
        }

        public override string ToString()
        {
            return string.Format("ChatRoom('{0}' ({1}), {2}/{3})", Name, ID, UserCount, MaxUsers);
        }
    }
}
