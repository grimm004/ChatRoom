using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;

namespace ChatRoom.Pages
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Page
    {
        private ChatRoomClient client;
        private Action ShowMenuPage;
        private Action ShowLoginPage;

        public Register(ChatRoomClient client, Action ShowLoginPage, Action ShowMenuPage)
        {
            this.ShowLoginPage = ShowLoginPage;
            this.ShowMenuPage = ShowMenuPage;
            InitializeComponent();

            this.KeyDown += new KeyEventHandler(KeyPressHandler);
            this.client = client;
        }

        private void KeyPressHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) StartRegister();
        }

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            StartRegister();
        }

        private void StartRegister()
        {
            backButton.IsEnabled = registerButton.IsEnabled = false;
            string host = SettingsManager.Current.Host;
            ushort port = SettingsManager.Current.Port;

            string username = usernameEntry.Text;
            string password = passwordEntry.Password;
            string confirmPassword = confirmPasswordEntry.Password;

            try
            {
                // Data validation
                if (password != confirmPassword) throw new PasswordsDoNotMatchException();
                if (username.Length < 4) throw new UsernameTooShortException();
                if (username.Length > 16) throw new UsernameTooLongException();
                if (!username.All(char.IsLetterOrDigit)) throw new UsernameNotAlphanumericException();
                if (password.Length < 5) throw new PasswordTooShortException();
                if (password.Length > 20) throw new PasswordTooLongException();

                Thread registerThread = new Thread(new ParameterizedThreadStart(delegate { ThreadedRegister(host, port, username, password); }));
                registerThread.IsBackground = true;
                registerThread.Start();
            }
            catch (RegisterException error)
            {
                MessageBox.Show(error.Message, "Registration Error");
                backButton.IsEnabled = registerButton.IsEnabled = true;
            }
        }

        private void ThreadedRegister(string host, ushort port, string username, string password)
        {
            try
            {
                client.Register(host, port, username, password);

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
            Dispatcher.Invoke(delegate { backButton.IsEnabled = registerButton.IsEnabled = true; });
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            ShowLoginPage();
        }
    }

    public class RegisterException : Exception
    {
        public RegisterException(string message) : base(message) { }
    }

    public class UsernameLengthException : RegisterException
    {
        public UsernameLengthException(string message) : base(message) { }
        public UsernameLengthException() : base("Invalid username length.") { }
    }

    public class UsernameTooShortException : UsernameLengthException
    {
        public UsernameTooShortException() : base("Username too short (minimum length 4 characters).") { }
    }

    public class UsernameTooLongException : UsernameLengthException
    {
        public UsernameTooLongException() : base("Username too long (maximum length 16 characters).") { }
    }

    public class UsernameNotAlphanumericException : RegisterException
    {
        public UsernameNotAlphanumericException() : base("Username must be alphanumeric.") { }
    }

    public class PasswordLengthException : RegisterException
    {
        public PasswordLengthException(string message) : base(message) { }
        public PasswordLengthException() : base("Invalid password length.") { }
    }

    public class PasswordTooShortException : PasswordLengthException
    {
        public PasswordTooShortException() : base("Password too short (minimum length 5 characters).") { }
    }

    public class PasswordTooLongException : PasswordLengthException
    {
        public PasswordTooLongException() : base("Password too long (maximum length 20 characters).") { }
    }

    public class PasswordsDoNotMatchException : PasswordLengthException
    {
        public PasswordsDoNotMatchException() : base("Passwords do not match.") { }
    }
}
