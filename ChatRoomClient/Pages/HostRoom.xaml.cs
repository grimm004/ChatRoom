using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ChatRoom.Pages
{
    /// <summary>
    /// Interaction logic for HostRoom.xaml
    /// </summary>
    public partial class HostRoom : Page
    {
        private Action ShowMenuPage;
        private Action<string, string> CreateRoomCommand;

        public HostRoom(Action ShowMenuPage, Action<string, string> CreateRoomCommand)
        {
            this.ShowMenuPage = ShowMenuPage;
            this.CreateRoomCommand = CreateRoomCommand;

            InitializeComponent();
            this.KeyDown += new KeyEventHandler(KeyPressHandler);
        }

        private void KeyPressHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) CreateRoom();
        }

        private void CreateRoom()
        {
            string roomName = roomNameEntry.Text;
            string roomPassword = passwordEntry.Text;

            if (!string.IsNullOrWhiteSpace(roomName)) CreateRoomCommand(roomName, roomPassword);
            else MessageBox.Show("Room name cannot be empty.", "Error");
        }

        private void createRoomButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(delegate { CreateRoom(); });
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMenuPage();
        }
    }
}
