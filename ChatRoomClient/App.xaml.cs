using System.Windows;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Win32;

namespace ChatRoom
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            try
            {
                SettingsManager.Load(Constants.ConfigFile);
                LoginInfo.Load();
            }
            catch (FileNotFoundException)
            {
                SettingsManager.LoadDefaultSettings();
                SettingsManager.Save(Constants.ConfigFile);
            }
        }
    }

    public class Constants
    {
        public const string ConfigFile = "client.xml";
        public const string LoginInfoSubKey = "SOFTWARE\\ChatRoomClient";
        public const int MaxMessageLength = 1024;
    }

    public static class SettingsManager
    {
        public static Settings Current { get; private set; }

        static SettingsManager()
        {
            LoadDefaultSettings();
        }

        public static void Load(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (FileStream file = new FileStream(fileName, FileMode.Open))
                Current = (Settings)serializer.Deserialize(file);
        }

        public static void Load(Settings settings)
        {
            Current = settings;
        }

        public static void LoadDefaultSettings()
        {
            Current = new Settings { Host = "127.0.0.1", Port = 8000 };
        }

        public static void Save(string fileName)
        {
            Save(fileName, Current);
        }

        public static void Save(string fileName, Settings settings)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (FileStream file = new FileStream(fileName, FileMode.Create))
                serializer.Serialize(file, settings);
        }
    }

    public class Settings
    {
        public string Host { get; set; }
        public ushort Port { get; set; }
    }

    public static class LoginInfo
    {
        public static string Username { get; set; }
        public static string Password { get; set; }
        
        public static void Load()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(Constants.LoginInfoSubKey))
            {
                if (key != null)
                {
                    Username = (string)key.GetValue("username", "");
                    Password = (string)key.GetValue("password", "");
                }
                else
                {
                    Save("", "");
                }
            }
        }

        public static void Save()
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(Constants.LoginInfoSubKey))
            {
                key.SetValue("username", Username, RegistryValueKind.String);
                key.SetValue("password", Password, RegistryValueKind.String);
            }
        }

        public static void Save(string username, string password)
        {
            Username = username;
            Password = password;
            Save();
        }
    }
}
