using System.Windows;

namespace ChatRoom.Pages
{
    /// <summary>
    /// Interaction logic for RoomPasswordEntry.xaml
    /// </summary>
    public partial class RoomPasswordEntry : Window
    {
        public string LatestPassword { get; private set; }
        public bool PasswordEntered { get; private set; }

        public RoomPasswordEntry()
        {
            InitializeComponent();
            ResizeMode = ResizeMode.CanMinimize;
            LatestPassword = "";
            PasswordEntered = false;
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            LatestPassword = passwordEntryBox.Password;
            PasswordEntered = true;
            Close();
        }

        public void Reset()
        {
            passwordEntryBox.Password = "";
            LatestPassword = "";
            PasswordEntered = false;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            Close();
        }
    }
}
