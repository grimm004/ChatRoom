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

namespace ChatRoom.Pages
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : Page
    {
        private Action LogoutCommand;
        private Action ShowHostRoomPage;
        private Action ServerListPage;

        public Menu(Action LogoutCommand, Action ShowHostRoomPage, Action ServerListPage)
        {
            this.LogoutCommand = LogoutCommand;
            this.ShowHostRoomPage = ShowHostRoomPage;
            this.ServerListPage = ServerListPage;
            InitializeComponent();
        }

        private void logountButton_Click(object sender, RoutedEventArgs e)
        {
            LogoutCommand();
        }

        private void hostRoomButton_Click(object sender, RoutedEventArgs e)
        {
            ShowHostRoomPage();
        }

        private void joinRoomButton_Click(object sender, RoutedEventArgs e)
        {
            LoadRoomList();
        }

        private void LoadRoomList()
        {
            ServerListPage();
        }
    }
}
