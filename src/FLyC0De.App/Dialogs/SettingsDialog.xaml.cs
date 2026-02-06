using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FLyC0De.App.Dialogs
{
    public partial class SettingsDialog : Window
    {
        public string SelectedLanguage { get; private set; }
        public bool MinimizeToTrayResult { get; private set; }
        public string SelectedTargetBot { get; private set; }
        public int SelectedBotPort { get; private set; } = 8080;

        public SettingsDialog(string currentLanguage, bool currentMinimizeToTray, string currentTargetBot, int currentBotPort)
        {
            InitializeComponent();

            var languages = new List<LanguageOption>
            {
                new LanguageOption { Name = "English", Code = "en" },
                new LanguageOption { Name = "Español", Code = "es" }
            };

            LanguageComboBox.ItemsSource = languages;
            LanguageComboBox.SelectedValue = currentLanguage;
            MinimizeToTrayCheckBox.IsChecked = currentMinimizeToTray;

            // Set Target Bot
            foreach (ComboBoxItem item in BotTargetComboBox.Items)
            {
                if (item.Content.ToString() == currentTargetBot)
                {
                    BotTargetComboBox.SelectedItem = item;
                    break;
                }
            }
            // Fallback default
            if (BotTargetComboBox.SelectedItem == null && BotTargetComboBox.Items.Count > 0)
                BotTargetComboBox.SelectedIndex = 0;

            BotPortTextBox.Text = currentBotPort.ToString();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedLanguage = LanguageComboBox.SelectedValue?.ToString();
            MinimizeToTrayResult = MinimizeToTrayCheckBox.IsChecked == true;
            
            if (BotTargetComboBox.SelectedItem is ComboBoxItem item)
            {
                SelectedTargetBot = item.Content.ToString();
            }

            if (int.TryParse(BotPortTextBox.Text, out int port))
            {
                SelectedBotPort = port;
            }
            
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class LanguageOption
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
