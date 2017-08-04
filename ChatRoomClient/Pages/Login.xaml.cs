using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;

namespace ChatRoom.Pages
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        private Action ShowRegisterScreen;
        private Action ShowMenuPage;
        private ChatRoomClient client;

        public Login(ChatRoomClient client, Action ShowRegisterScreen, Action ShowMenuPage)
        {
            this.client = client;
            this.ShowMenuPage = ShowMenuPage;
            this.ShowRegisterScreen = ShowRegisterScreen;
            InitializeComponent();

            UpdateLoginInfo();

            this.KeyDown += new KeyEventHandler(KeyPressHandler);
        }

        public void UpdateLoginInfo()
        {
            usernameEntry.Text = LoginInfo.Username;
            passwordEntry.Password = LoginInfo.Password;
        }

        private void KeyPressHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) StartLogin();
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            StartLogin();
        }

        private void StartLogin()
        {
            loginButton.IsEnabled = registerButton.IsEnabled = false;

            string host = SettingsManager.Current.Host;
            ushort port = SettingsManager.Current.Port;

            string username = usernameEntry.Text;
            string password = passwordEntry.Password;

            try
            {
                if (string.IsNullOrWhiteSpace(username)) throw new UsernameRequiredException();
                if (string.IsNullOrWhiteSpace(password)) throw new PasswordRequiredException();

                Thread loginThread = new Thread(new ParameterizedThreadStart(delegate { LoginThreaded(host, port, username, password); }));
                loginThread.IsBackground = true;
                loginThread.Start();
            }
            catch (InvalidUsernameOrPasswordException error)
            {
                MessageBox.Show(error.Message, "Login Error");
                loginButton.IsEnabled = registerButton.IsEnabled = true;
            }
        }

        private void LoginThreaded(string host, ushort port, string username, string password)
        {
            try
            {
                client.Login(host, port, username, password);
                switch (client.LatestLoginAttemtResponse)
                {
                    case LoginExchangeResponseCode.Successful:
                        Dispatcher.Invoke(delegate { ShowMenuPage(); });
                        LoginInfo.Save(username, password);
                        break;
                    case LoginExchangeResponseCode.InvalidUsernameOrPassword:
                        throw new InvalidUsernameOrPasswordException();
                    case LoginExchangeResponseCode.ConnectionError:
                        throw new ConnectionErrorException();
                    case LoginExchangeResponseCode.AlreadyLoggedIn:
                        throw new AlreadyLoggedInException();
                    case LoginExchangeResponseCode.ServerFull:
                        throw new ServerFullException();
                    case LoginExchangeResponseCode.GenericError:
                    default:
                        throw new LoginException();
                }
            }
            catch (LoginException error)
            {
                MessageBox.Show(error.Message, "Login Error");
            }
            SafeReset();
        }

        private void SafeReset()
        {
            Dispatcher.Invoke(delegate { loginButton.IsEnabled = registerButton.IsEnabled = true; });
        }

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            ShowRegisterScreen();
        }
    }
}
