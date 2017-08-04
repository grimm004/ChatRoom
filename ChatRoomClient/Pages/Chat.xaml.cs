using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Documents;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace ChatRoom.Pages
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : Page
    {
        public Color Red = Color.FromRgb(0xFF, 0x00, 0x00);
        public Color Green = Color.FromRgb(0x00, 0xFF, 0x00);
        public Color Blue = Color.FromRgb(0x00, 0x00, 0xFF);
        
        private Action<string> SendMessageAction;
        private Action LeaveRoomCommand;

        public Chat(Action<string> SendMessageAction, Action LeaveRoomCommand)
        {
            this.LeaveRoomCommand = LeaveRoomCommand;
            this.SendMessageAction = SendMessageAction;

            InitializeComponent();
            this.KeyDown += new KeyEventHandler(KeyPressHandler);
            connectionStatusLabel.IsEnabled = false;
            connectionStatusLabel.Foreground = new SolidColorBrush(Red);
            messageEntry.IsEnabled = false;
            sendButton.IsEnabled = false;
            messageEntry.MaxLength = Constants.MaxMessageLength;
            OutputText("", false, true);
        }

        public void SafeEnable()
        {
            Dispatcher.Invoke(delegate { Enable(); });
        }

        public void Enable()
        {
            messageEntry.IsEnabled = true;
            sendButton.IsEnabled = true;
            connectionStatusLabel.Content = "Connected";
            connectionStatusLabel.Foreground = new SolidColorBrush(Green);
        }

        public void SafeDisable()
        {
            Dispatcher.Invoke(delegate { Disable(); });
        }

        public void Disable()
        {
            messageEntry.IsEnabled = false;
            sendButton.IsEnabled = false;
            messageEntry.Text = "";
            connectionStatusLabel.Content = "Disconnected";
            connectionStatusLabel.Foreground = new SolidColorBrush(Red);
        }

        public void OnReceive(ChatExchange exchange)
        {
            string username = exchange.SenderName;
            string message = exchange.Exchange;

            OutputTextSafe(string.Format("{0} {1}", username, message), true, true, true);
        }

        private void KeyPressHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) SendMessage();
        }

        private void ReceiveAction(string message) { OutputTextSafe(message, true, true, true); }

        public void OutputTextSafe(string text, bool showTime = true, bool newLine = true, bool scrollToEnd = true)
        {
            chatLogBox.Dispatcher.Invoke(delegate { this.OutputText(text, showTime, newLine, scrollToEnd); });
        }

        public void OutputText(string text, bool showTime = true, bool newLine = true, bool scrollToEnd = true)
        {
            chatLogBox.AppendText(string.Format("{0}{1}", showTime ? string.Format("{0} - ", ChatTime.GetTimeFormat()) : "", text) + (newLine ? Environment.NewLine : ""));
            if (scrollToEnd) chatLogBox.ScrollToEnd();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            string message = messageEntry.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                SendMessageAction(message);
                messageEntry.Text = "";
            }
        }

        public void SafeClearChat()
        {
            Dispatcher.Invoke(delegate { ClearChat(); });
        }

        public void ClearChat()
        {
            chatLogBox.Document.Blocks.Clear();
            OutputText("", false, true);
        }

        private void messageEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            characterCountLabel.Content = string.Format("{0}/{1}", Encoding.UTF8.GetByteCount(messageEntry.Text), Constants.MaxMessageLength);
        }

        private void LeaveRoomClick(object sender, RoutedEventArgs e)
        {
            LeaveRoomCommand();
        }

        private void ClearChatClick(object sender, RoutedEventArgs e)
        {
            ClearChat();
        }

        private void SaveChatLogClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = string.Format("chatlog-{0}", ChatTime.GetTimeFileFormat()); // Default file name
            dlg.DefaultExt = ".xaml"; // Default file extension
            dlg.Filter = "XAML Document (.xaml)|*.xaml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;

                string richText = new TextRange(chatLogBox.Document.ContentStart, chatLogBox.Document.ContentEnd).Text;

                FileStream docStream = new FileStream(filename, FileMode.OpenOrCreate);
                System.Windows.Markup.XamlWriter.Save(chatLogBox.Document, docStream);
                docStream.Close();
            }
        }
    }
}
