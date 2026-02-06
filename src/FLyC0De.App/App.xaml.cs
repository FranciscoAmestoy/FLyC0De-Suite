using System.Windows;
using System.Threading;
using FLyC0De.Core.Configuration;

namespace FLyC0De.App
{
    public partial class App : Application
    {
        private static Mutex _mutex;

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Single instance check
            const string mutexName = "FLyC0De_SingleInstance_Mutex";
            bool createdNew;
            _mutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("There's already a running instance of FLyC0De Suite.", "FLyC0De Suite", MessageBoxButton.OK, MessageBoxImage.Warning);
                Shutdown();
                return;
            }

            // Load Configuration to determine language
            var configManager = new Core.Configuration.ConfigurationManager();
            configManager.Load(); // Ensure this doesn't throw or handles missing config gracefully

            string lang = configManager.Config.Settings.Language;
            if (string.IsNullOrEmpty(lang)) lang = "en";

            var dict = new ResourceDictionary();
            if (lang == "es")
            {
                dict.Source = new System.Uri("pack://application:,,,/Resources/Strings.es.xaml");
            }
            else
            {
                dict.Source = new System.Uri("pack://application:,,,/Resources/Strings.xaml");
            }
            
            // Add to MergedDictionaries. 
            // Note: App.xaml already has a ResourceDictionary. We should append or insert.
            // Inserting at 0 might override defaults if keys match, appending adds if missing.
            Resources.MergedDictionaries.Add(dict);

            var mainWindow = new Views.MainWindow();
            mainWindow.Show();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}\n\n{e.Exception.StackTrace}", "FLyC0De Crash", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
