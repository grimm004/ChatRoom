using System;
using System.Windows;
using System.Windows.Controls;
using System.Net.Sockets;
using System.Threading;

namespace ChatRoom
{
    /// <summary>
    /// Interaction logic for Master.xaml
    /// </summary>
    public partial class Master : Window
    {
        private Pages.Login loginPage;
        private Pages.Register registerPage;
        private Pages.Menu menuPage;
        private Pages.HostRoom hostRoomPage;
        private Pages.Chat chatPage;
        private Pages.RoomList roomBrowser;

        private ChatRoomClient client;

        private bool pokeEnabled;

        public Master()
        {
            InitializeComponent();

            pokeEnabled = true;

            client = new ChatRoomClient(CommandResponseHandler);

            loginPage = new Pages.Login(client, ShowRegisterPage, ShowMenuPage);
            registerPage = new Pages.Register(client, ShowLoginPage, ShowMenuPage);
            menuPage = new Pages.Menu(Logout, ShowHostRoomPage, ShowServerListPage);
            hostRoomPage = new Pages.HostRoom(ShowMenuPage, HostRoom);
            chatPage = new Pages.Chat(SendMessageHandler, LeaveRoom);
            roomBrowser = new Pages.RoomList(client, ShowMenuPage, JoinRoom, UpdateServerListSize);

            ShowLoginPage();
        }

        public void ShowLoginPage()
        {
            ResizeMode = ResizeMode.CanMinimize;
            loginPage.UpdateLoginInfo();
            ShowPage(loginPage);
            chatPage.SafeClearChat();
        }

        public void ShowRegisterPage()
        {
            ResizeMode = ResizeMode.CanMinimize;
            ShowPage(registerPage);
        }

        public void ShowMenuPage()
        {
            ResizeMode = ResizeMode.CanMinimize;
            ShowPage(menuPage);
        }

        public void ShowHostRoomPage()
        {
            ResizeMode = ResizeMode.CanMinimize;
            ShowPage(hostRoomPage);
        }

        public void ShowServerListPage()
        {
            try
            { 
                ResizeMode = ResizeMode.CanMinimize;
                ShowPage(roomBrowser);
                roomBrowser.RefreshServerList();
            }
            catch (SocketException)
            {
                MessageBox.Show("Disconnected from server.", "Connection Error");
                ShowLoginPage();
            }
        }

        public void SendMessageHandler(string message)
        {
            if (message[0] != '/')
                client.SendMessage(message);
            else if (message[0] == '/' && message.Length > 1) LocalCommandHandler(message.Substring(1));
        }

        public void LocalCommandHandler(string fullCommand)
        {
            string command = fullCommand.Split(' ')[0];
            switch (command.ToLower())
            {
                case "clear":
                    chatPage.SafeClearChat();
                    break;
                case "disablepoke":
                    pokeEnabled = false;
                    chatPage.OutputTextSafe("Disabled poking.", false);
                    break;
                case "enablepoke":
                    pokeEnabled = true;
                    chatPage.OutputTextSafe("Enabled poking.", false);
                    break;
                case "poke":
                    if (pokeEnabled) client.SendCommand(fullCommand);
                    else chatPage.OutputTextSafe("Poking is not enabled. To enable, type '/enablepoke'...", false);
                    break;
                default:
                    client.SendCommand(fullCommand);
                    break;
            }
        }

        public void CommandResponseHandler(CommandExchangeResponse response)
        {
            switch (response.ResponseType)
            {
                case CommandResponse.Success:
                    chatPage.OutputTextSafe(string.Format("[Server] {0}", response.OutputMessage));
                    break;
                case CommandResponse.UnknownCommand:
                    chatPage.OutputTextSafe("Unknown command. Type '/help' for a list of commands.");
                    break;
                case CommandResponse.Fail:
                    chatPage.OutputTextSafe(string.Format("[Server] {0}", response.OutputMessage));
                    break;
                default:
                    chatPage.OutputTextSafe("Error executing command.");
                    break;
            }
        }

        public void UpdateServerListSize()
        {
            Height = roomBrowser.currentHeight;
            Width = roomBrowser.currentHeight;
        }

        public void ShowChatPage()
        {
            try
            {
                ResizeMode = ResizeMode.CanResize;
                ShowPage(chatPage);
                Width = 450;
                Height = 350;
                client.StartThreadedListen(chatPage.OnReceive, OnCommandReceive, ExceptionAction);
                chatPage.Enable();
            }
            catch (SocketException)
            {
                MessageBox.Show("Disconnected from server.", "Connection Error");
                ShowLoginPage();
            }
        }

        public void OnCommandReceive(ChatExchange exchange)
        {
            switch (exchange.ExchangeTargetName)
            {
                case "poke":
                    if (pokeEnabled)
                    {
                        chatPage.OutputTextSafe(string.Format("Poked by '{0}'...", exchange.SenderName));
                        new Thread(new ThreadStart(delegate { MessageBox.Show(exchange.Exchange, "You have been poked!"); })).Start();
                    }
                    break;
                default:
                    break;
            }
        }

        public void LeaveRoom()
        {
            try
            {
                client.LeaveRoom();
                chatPage.SafeClearChat();
                chatPage.SafeDisable();
                ShowMenuPage();
            }
            catch (SocketException)
            {
                MessageBox.Show("Disconnected from server.", "Connection Error");
                ShowLoginPage();
            }
        }

        public void ExceptionAction(Exception exception)
        {
            MessageBox.Show("Disconnected from server.", "Connection Error");
            Dispatcher.Invoke(delegate { chatPage.Disable(); ShowLoginPage(); });
        }

        public void ShowPage(Page page, bool setSize = true)
        {
            container.Content = page;
            if (setSize) Width = page.Width;
            if (setSize) Height = page.Height;
        }

        public void HostRoom(string roomName, string roomPassword)
        {
            JoinRoomOutcome outcome = client.CreateRoom(roomName, roomPassword);
            switch (outcome)
            {
                case JoinRoomOutcome.Success:
                    ShowChatPage();
                    chatPage.OutputTextSafe(string.Format("Joined '{0}', type '/help' for a list of commnads.", roomName), false);
                    break;
                case JoinRoomOutcome.Fail:
                    MessageBox.Show("Could not join the room.", "Join Error");
                    break;
            }
        }

        public void JoinRoom(AvailableRoom room)
        {
            string password = string.Empty;
            bool attemptJoin = true;
            if (room.RequiresPassword)
            {
                Pages.RoomPasswordEntry passwordEntryWindow = new Pages.RoomPasswordEntry();
                passwordEntryWindow.ShowDialog();
                password = passwordEntryWindow.LatestPassword;
                attemptJoin = passwordEntryWindow.PasswordEntered;
                passwordEntryWindow.Reset();
                passwordEntryWindow = null;
            }

            if (attemptJoin)
            {
                JoinRoomOutcome outcome = client.JoinRoom(room, password);
                switch (outcome)
                {
                    case JoinRoomOutcome.Success:
                        ShowChatPage();
                        chatPage.OutputTextSafe(string.Format("Joined '{0}', type '/help' for a list of commnads.", room.Name), false);
                        break;
                    case JoinRoomOutcome.Fail:
                        MessageBox.Show("Could not join the room.", "Join Error");
                        break;
                    case JoinRoomOutcome.InvalidPassword:
                        MessageBox.Show("Invalid password.", "Join Error");
                        break;
                }
            }
        }

        public void Logout()
        {
            client.Logout();
            ShowLoginPage();
        }

        public void SetContainerSize(double width, double height)
        {
            container.Width = Width = width;
            container.Height = Height = height;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            client.Logout();
            Application.Current.Shutdown();
        }
    }
}
