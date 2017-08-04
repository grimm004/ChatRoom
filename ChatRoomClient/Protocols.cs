using System;
using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace ChatRoom
{
    #region Exchange
    public abstract class Exchange
    {
        public string Encode()
        {
            return JsonConvert.SerializeObject(this);
        }

        public byte[] EncodeBytes()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }

        public static implicit operator byte[](Exchange exchange)
        {
            return exchange.EncodeBytes();
        }
    }
    #endregion

    #region ChatExchange
    public class ChatExchange : Exchange
    {
        public string LoginToken { get; set; }
        public ChatExchangeType ExchangeType { get; set; }
        public ChatExchangeTarget ExchangeTarget { get; set; }
        public string SenderName { get; set; }
        public string ExchangeTargetName { get; set; }
        public string SendTimeString { get { return ChatTime.GetTimeFormat(); } }
        public string Exchange { get; set; }

        public static string Encode(string loginToken, ChatExchangeType exchangeType, ChatExchangeTarget exchangeTarget, string senderName, string exchangeTargetName, string exchange)
        {
            return JsonConvert.SerializeObject(new ChatExchange { LoginToken = loginToken, ExchangeType = exchangeType, ExchangeTarget = exchangeTarget, SenderName = senderName, ExchangeTargetName = exchangeTargetName, Exchange = exchange });
        }

        public static ChatExchange Decode(string encodedChatExchange)
        {
            return JsonConvert.DeserializeObject<ChatExchange>(encodedChatExchange);
        }

        public static ChatExchange Decode(byte[] utf8EncodedChatExchange, int length)
        {
            return JsonConvert.DeserializeObject<ChatExchange>(Encoding.UTF8.GetString(utf8EncodedChatExchange, 0, length));
        }
    }

    public enum ChatExchangeType
    {
        Message,
        Command,
        ChatRoomList,
        JoinRoom,
        CreateRoom,
        LeaveRoom,
        Error
    }

    public enum ChatExchangeTarget
    {
        ChatRoom,
        Individual,
        Server,
    }
    #endregion

    public class CommandExchangeResponse : Exchange
    {
        public CommandResponse ResponseType { get; set; }
        public string OutputMessage { get; set; }
        public string FurtherInfo { get; set; }

        public static CommandExchangeResponse Decode(string encodedCommandResponseExchange)
        {
            return JsonConvert.DeserializeObject<CommandExchangeResponse>(encodedCommandResponseExchange);
        }

        public static CommandExchangeResponse Decode(byte[] utf8EncodedChatExchange, int length)
        {
            return JsonConvert.DeserializeObject<CommandExchangeResponse>(Encoding.UTF8.GetString(utf8EncodedChatExchange, 0, length));
        }
    }

    public enum CommandResponse
    {
        Success,
        Fail,
        AccessDenied,
        UnknownCommand,
    }

    #region RoomListExchange
    public class RoomListExchange : Exchange
    {
        public List<AvailableRoom> Rooms { get; set; }

        public static string Encode(List<AvailableRoom> rooms)
        {
            return JsonConvert.SerializeObject(new RoomListExchange { Rooms = rooms });
        }

        public static RoomListExchange Decode(string encodedChatExchange)
        {
            return JsonConvert.DeserializeObject<RoomListExchange>(encodedChatExchange);
        }

        public static RoomListExchange Decode(byte[] utf8EncodedChatExchange, int length)
        {
            return JsonConvert.DeserializeObject<RoomListExchange>(Encoding.UTF8.GetString(utf8EncodedChatExchange, 0, length));
        }
    }

    public class AvailableRoom
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public int UserCount { get; set; }
        public int MaxUsers { get; set; }
        public bool RoomFull { get { return UserCount >= MaxUsers; } }
        public bool RequiresPassword { get; set; }

        public override string ToString()
        {
            return string.Format("Room('{0}' ({1}), {2}/{3})", Name, ID, UserCount, MaxUsers);
        }
    }
    #endregion

    #region LoginExchange
    public class LoginExchange : Exchange
    {
        public string LoginToken { get; set; }
        public LoginExchangeType ExchangeType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public static string Encode(LoginExchangeType exchangeType, string username, string password, string loginToken = "")
        {
            return JsonConvert.SerializeObject(new LoginExchange { ExchangeType = exchangeType, Username = username, Password = Hash.GetSha256hString(password) });
        }

        public static LoginExchange Decode(string encodedChatExchange)
        {
            return JsonConvert.DeserializeObject<LoginExchange>(encodedChatExchange);
        }

        public static LoginExchange Decode(byte[] utf8EncodedChatExchange, int length)
        {
            return JsonConvert.DeserializeObject<LoginExchange>(Encoding.UTF8.GetString(utf8EncodedChatExchange, 0, length));
        }
    }

    public enum LoginExchangeType
    {
        Login,
        Register,
        Logout,
    }
    #endregion

    #region LoginExchangeResponse
    public class LoginExchangeResponse : Exchange
    {
        public string LoginToken { get; set; }
        public LoginExchangeResponseCode ExchangeType { get; set; }

        public static string Encode(LoginExchangeResponseCode responseCode, string loginToken = "")
        {
            return JsonConvert.SerializeObject(new LoginExchangeResponse { LoginToken = loginToken, ExchangeType = responseCode });
        }

        public static LoginExchangeResponse Decode(string encodedChatExchange)
        {
            return JsonConvert.DeserializeObject<LoginExchangeResponse>(encodedChatExchange);
        }

        public static LoginExchangeResponse Decode(byte[] utf8EncodedChatExchange, int length)
        {
            return JsonConvert.DeserializeObject<LoginExchangeResponse>(Encoding.UTF8.GetString(utf8EncodedChatExchange, 0, length));
        }
    }

    public enum LoginExchangeResponseCode
    {
        Successful,
        InvalidUsernameOrPassword,
        UsernameExists,
        AlreadyLoggedIn,
        ConnectionError,
        ServerFull,
        GenericError,
    }
    #endregion

    public class ChatTime
    {
        public static string GetTimeFormat()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        public static string GetTimeFileFormat()
        {
            return DateTime.Now.ToString("HH-mm-ss");
        }
    }

    public class Hash
    {
        public static string GetSha256hString(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash) hashString += String.Format("{0:x2}", x);
            return hashString;
        }
    }
}
