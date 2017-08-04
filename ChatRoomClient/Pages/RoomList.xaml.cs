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
using System.Net.Sockets;

namespace ChatRoom.Pages
{
    /// <summary>
    /// Interaction logic for RoomList.xaml
    /// </summary>
    public partial class RoomList : Page
    {
        private ChatRoomClient loginSession;
        private Action ShowMenuPage;
        private Action<AvailableRoom> JoinRoomCommand;
        private Action UpdateListSize;

        public int currentHeight;
        public int currentWidth;

        private List<UIElement> addedUIElements;

        public RoomList(ChatRoomClient loginSession, Action ShowMenuPage, Action<AvailableRoom> JoinRoomCommand, Action UpdateListSize)
        {
            this.UpdateListSize = UpdateListSize;
            this.JoinRoomCommand = JoinRoomCommand;
            this.ShowMenuPage = ShowMenuPage;
            this.loginSession = loginSession;
            InitializeComponent();
            addedUIElements = new List<UIElement>();
        }

        public void RefreshServerList()
        {
            Reset();

            addedUIElements = new List<UIElement>();

            AvailableRoom[] rooms = loginSession.GetAvailableRooms();

            Label connectButtonColumnLabel = new Label() { Content = "Name", Width = 100, Height = 25, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 0, 0, 0) };
            AddElement(connectButtonColumnLabel);

            Label serverInfoColumnLabel = new Label() { Content = "Room Info", Width = 100, Height = 25, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(100, 0, 0, 0) };
            AddElement(serverInfoColumnLabel);

            currentHeight = 25;
            int rowHeight = 25;
            foreach (AvailableRoom room in rooms)
            {
                AddRow(rowHeight, room);
                currentHeight += rowHeight;
            }
            serverListGrid.Height = currentHeight;
            currentHeight += 75;
            currentWidth = 225;
            UpdateListSize();
        }

        private void AddElement(UIElement element)
        {
            addedUIElements.Add(element);
            serverListGrid.Children.Add(element);
        }

        private void Reset()
        {
            foreach (UIElement element in addedUIElements) serverListGrid.Children.Remove(element);
        }

        private void AddRow(int rowHeight, AvailableRoom room)
        {
            Button connectButton = new Button()
            {
                Content = room.Name,
                Width = 100,
                Height = rowHeight,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, currentHeight, 0, 0)
            };
            connectButton.Click += delegate { JoinRoomCommand(room); };
            AddElement(connectButton);

            Label infoLabel = new Label()
            {
                Content = string.Format("{0}/{1}", room.UserCount, room.MaxUsers),
                Width = 100,
                Height = rowHeight,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(100, currentHeight, 0, 0)
            };
            AddElement(infoLabel);

            infoLabel = new Label()
            {
                Content = room.RequiresPassword ? "Requires Password" : "No Password",
                Width = 125,
                Height = rowHeight,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(200, currentHeight, 0, 0)
            };
            AddElement(infoLabel);
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMenuPage();
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshServerList();
        }
    }
}
