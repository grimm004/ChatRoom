using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace ChatRoom
{
    public class ChatRoomClient
    {
        public bool LoggedIn { get; private set; }
        public string Username { get; private set; }
        public string LoginToken { get; private set; }

        private Socket socket;

        private Action<CommandExchangeResponse> CommandResponseHandler;

        public LoginExchangeResponseCode LatestLoginAttemtResponse { get; private set; }

        public ChatRoomClient(Action<CommandExchangeResponse> CommandResponseHandler)
        {
            this.CommandResponseHandler = CommandResponseHandler;
            Initialize();
            LatestLoginAttemtResponse = LoginExchangeResponseCode.GenericError;
            LoggedIn = false;
        }

        public void Initialize()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public bool Connect(string host, ushort port)
        {
            try
            {
                Initialize();
                socket.Connect(host, port);

                return true;
            }
            catch (SocketException)
            {
                LatestLoginAttemtResponse = LoginExchangeResponseCode.ConnectionError;
                return false;
            }
        }

        public bool Login(string host, ushort port, string username, string password)
        {
            Connect(host, port);

            if (socket.Connected)
            {
                socket.Send(Encoding.UTF8.GetBytes(LoginExchange.Encode(LoginExchangeType.Login, username, password)));
                byte[] responseBuffer = new byte[1024 * 2];
                int responseSize = socket.Receive(responseBuffer);
                LoginExchangeResponse response = LoginExchangeResponse.Decode(responseBuffer, responseSize);
                LatestLoginAttemtResponse = response.ExchangeType;
                switch (response.ExchangeType)
                {
                    case LoginExchangeResponseCode.Successful:
                        LoggedIn = true;
                        Username = username;
                        LoginToken = response.LoginToken;
                        return true;
                    default:
                        LoggedIn = false;
                        return false;
                }
            }
            else
            {
                LoggedIn = false;
                return false;
            }
        }

        public bool Register(string host, ushort port, string username, string password)
        {
            Connect(host, port);

            if (socket.Connected)
            {
                socket.Send(Encoding.UTF8.GetBytes(LoginExchange.Encode(LoginExchangeType.Register, username, password)));
                byte[] responseBuffer = new byte[1024 * 2];
                int responseSize = socket.Receive(responseBuffer);
                LoginExchangeResponse response = LoginExchangeResponse.Decode(responseBuffer, responseSize);
                LatestLoginAttemtResponse = response.ExchangeType;
                switch (LatestLoginAttemtResponse)
                {
                    case LoginExchangeResponseCode.Successful:
                        LoggedIn = true;
                        Username = username;
                        LoginToken = response.LoginToken;
                        return true;
                    default:
                        LoggedIn = false;
                        return false;
                }
            }
            else
            {
                LoggedIn = false;
                return false;
            }
        }

        public AvailableRoom[] GetAvailableRooms()
        {
            socket.Send(Encoding.UTF8.GetBytes(ChatExchange.Encode(LoginToken, ChatExchangeType.ChatRoomList, ChatExchangeTarget.Server, "", "", "")));
            byte[] receiveBuffer = new byte[1024 * 8];
            int receiveSize = socket.Receive(receiveBuffer);
            RoomListExchange exchange = RoomListExchange.Decode(receiveBuffer, receiveSize);
            List<AvailableRoom> rooms = exchange.Rooms;
            return rooms.ToArray();
        }

        public JoinRoomOutcome CreateRoom(string roomName, string roomPassword)
        {
            socket.Send(Encoding.UTF8.GetBytes(ChatExchange.Encode(LoginToken, ChatExchangeType.CreateRoom, ChatExchangeTarget.Server, "", roomName, roomPassword)));
            byte[] receiveBuffer = new byte[1024 * 2];
            int receiveSize = socket.Receive(receiveBuffer);
            ChatExchange exchange = ChatExchange.Decode(receiveBuffer, receiveSize);
            if (exchange.Exchange == "JOINED") return JoinRoomOutcome.Success;
            return JoinRoomOutcome.Fail;
        }

        public JoinRoomOutcome JoinRoom(AvailableRoom room, string password = "")
        {
            return JoinRoom(room.ID, password);
        }

        public JoinRoomOutcome JoinRoom(string roomId, string password = "")
        {
            socket.Send(Encoding.UTF8.GetBytes(ChatExchange.Encode(LoginToken, ChatExchangeType.JoinRoom, ChatExchangeTarget.Server, "", roomId, password)));
            byte[] receiveBuffer = new byte[1024 * 2];
            int receiveSize = socket.Receive(receiveBuffer);
            ChatExchange exchange = ChatExchange.Decode(receiveBuffer, receiveSize);
            if (exchange.Exchange == "JOINED") return JoinRoomOutcome.Success;
            if (exchange.Exchange == "INVALIDPASSWORD") return JoinRoomOutcome.InvalidPassword;
            return JoinRoomOutcome.Fail;
        }

        public void LeaveRoom()
        {
            socket.Send(Encoding.UTF8.GetBytes(ChatExchange.Encode(LoginToken, ChatExchangeType.LeaveRoom, ChatExchangeTarget.Server, "", "", "")));
        }

        public void SendMessage(string message)
        {
            socket.Send(Encoding.UTF8.GetBytes(ChatExchange.Encode(LoginToken, ChatExchangeType.Message, ChatExchangeTarget.ChatRoom, "", "", message)));
        }

        public void StartThreadedListen(Action<ChatExchange> receiveCommand, Action<ChatExchange> commandCommand, Action<Exception> ExceptionAction = null)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(delegate { StartListen(receiveCommand, commandCommand, ExceptionAction); }));
            thread.IsBackground = true;
            thread.Start();
        }


        bool expectingCommandResponse = false;
        public void SendCommand(string command)
        {
            ChatExchange exchange = new ChatExchange() { LoginToken = LoginToken, ExchangeType = ChatExchangeType.Command, ExchangeTargetName = "", SenderName = "", ExchangeTarget = ChatExchangeTarget.Server, Exchange = command };
            socket.Send(exchange);
            expectingCommandResponse = true;
        }

        public void StartListen(Action<ChatExchange> receiveCommand, Action<ChatExchange> commandCommand, Action<Exception> ExceptionAction = null)
        {
            try
            {
                bool doReceive = true;
                while (doReceive)
                {
                    byte[] receiveBuffer = new byte[1024 * 2];
                    int receiveSize = socket.Receive(receiveBuffer);
                    if (expectingCommandResponse) { CommandResponseHandler(CommandExchangeResponse.Decode(receiveBuffer, receiveSize)); expectingCommandResponse = false; }
                    else
                    {
                        ChatExchange chatExchange = ChatExchange.Decode(receiveBuffer, receiveSize);
                        switch (chatExchange.ExchangeType)
                        {
                            case ChatExchangeType.Message:
                                receiveCommand?.Invoke(chatExchange);
                                break;
                            case ChatExchangeType.Command:
                                if (chatExchange.Exchange == "LEFT") doReceive = false;
                                commandCommand?.Invoke(chatExchange);
                                continue;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                ExceptionAction?.Invoke(e);
            }
        }

        public void Logout()
        {
            if (socket.Connected && LoggedIn)
                socket.Send(Encoding.UTF8.GetBytes(LoginExchange.Encode(LoginExchangeType.Logout, "", "", LoginToken)));
            LoggedIn = false;
            if (socket.Connected) socket.Close();
        }
    }

    public enum JoinRoomOutcome
    {
        Success,
        Fail,
        InvalidPassword,
    }

    public class LoginException : Exception
    {
        public LoginException(string message) : base(message) { }
        public LoginException() : base("There was an error logging in.") { }
    }

    public class InvalidUsernameOrPasswordException : LoginException
    {
        public InvalidUsernameOrPasswordException() : base("Invalid username or password.") { }
        public InvalidUsernameOrPasswordException(string message) : base(message) { }
    }

    public class UsernameRequiredException : InvalidUsernameOrPasswordException
    {
        public UsernameRequiredException() : base("Username is requried.") { }
    }

    public class PasswordRequiredException : InvalidUsernameOrPasswordException
    {
        public PasswordRequiredException() : base("Password is requried.") { }
    }

    public class ConnectionErrorException : LoginException
    {
        public ConnectionErrorException() : base("Could not connect to server.") { }
    }

    public class AlreadyLoggedInException : LoginException
    {
        public AlreadyLoggedInException() : base("Already logged in.") { }
    }

    public class ServerFullException : LoginException
    {
        public ServerFullException() : base("Server is full.") { }
    }
}
