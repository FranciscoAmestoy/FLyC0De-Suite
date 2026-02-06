using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FLyC0De.Core.Configuration;
using FLyC0De.Core.Interception;
using FLyC0De.Core.Services;
using FLyC0De.Core.Actions;
using FLyC0De.Core.Interfaces;
using FLyC0De.App.Dialogs;

// Note: Dialogs namespaces will need to be adjusted once ported
namespace FLyC0De.App.Views
{
    public partial class MainWindow : Window
    {
        private readonly InterceptionService _interceptionService;
        private readonly ConfigurationManager _configManager;
        private readonly ActionEngine _actionEngine;
        private readonly PluginManager _pluginManager;

        private ObservableCollection<DeviceViewModel> _devices;
        private ObservableCollection<BindingViewModel> _bindings;

        public ICommand ShowWindowCommand { get; }

        public MainWindow()
        {
            InitializeComponent();

            _interceptionService = new InterceptionService();
            _configManager = new ConfigurationManager();
            _actionEngine = new ActionEngine();
            _pluginManager = new PluginManager();

            _devices = new ObservableCollection<DeviceViewModel>();
            _bindings = new ObservableCollection<BindingViewModel>();

            DeviceListBox.ItemsSource = _devices;
            BindingsListBox.ItemsSource = _bindings;

            ShowWindowCommand = new RelayCommand(ShowFromTray);
            DataContext = this; 
        }

        private void ShowFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void MyNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ShowFromTray();
        }

        private void TrayOpen_Click(object sender, RoutedEventArgs e)
        {
            ShowFromTray();
        }

        private void TrayExit_Click(object sender, RoutedEventArgs e)
        {
            _configManager.Save();
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_configManager.Config.Settings.MinimizeToTray)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                _interceptionService.Stop();
                _configManager.Save();
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SettingsDialog(
                _configManager.Config.Settings.Language,
                _configManager.Config.Settings.MinimizeToTray,
                _configManager.Config.Settings.TargetBot,
                _configManager.Config.Settings.BotPort);

            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                if (_configManager.Config.Settings.Language != dialog.SelectedLanguage)
                {
                    _configManager.Config.Settings.Language = dialog.SelectedLanguage;
                    _configManager.Save();
                    
                    var message = _configManager.Config.Settings.Language == "es" 
                            ? "El idioma ha cambiado. La aplicación necesita reiniciarse para aplicar los cambios. ¿Reiniciar ahora?" 
                            : "Language changed. The application needs to restart to apply changes. Restart now?";
                    var title = _configManager.Config.Settings.Language == "es" ? "Reinicio Requerido" : "Restart Required";

                    var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
                        // Use Process.Start to restart
                        if (System.IO.Path.GetExtension(exePath).ToLower() == ".dll")
                        {
                             // For .NET Core/5+ this might be a dll, but we are on .NET 4.8 Framework as per project file
                             // .NET Framework usually GetEntryAssembly().Location is the .exe
                             // Just in case double check
                             exePath = exePath.Replace(".dll", ".exe");
                        }
                        
                        System.Diagnostics.Process.Start(exePath);
                        Application.Current.Shutdown();
                    }
                }

                _configManager.Config.Settings.MinimizeToTray = dialog.MinimizeToTrayResult;
                _configManager.Config.Settings.TargetBot = dialog.SelectedTargetBot;
                _configManager.Config.Settings.BotPort = dialog.SelectedBotPort;
                _configManager.Save();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _configManager.Load();

                // Load Plugins
                _pluginManager.LoadPlugins();
                ModulesListBox.ItemsSource = _pluginManager.Plugins;

                _interceptionService.DeviceDetected += OnDeviceDetected;
                _interceptionService.KeyEvent += OnKeyEvent;

                foreach (var deviceConfig in _configManager.Config.Devices)
                {
                    _devices.Add(new DeviceViewModel
                    {
                        HardwareId = deviceConfig.HardwareId,
                        FriendlyName = deviceConfig.FriendlyName,
                        IsIntercepted = deviceConfig.IsIntercepted
                    });

                    if (deviceConfig.IsIntercepted)
                    {
                        _interceptionService.SetDeviceIntercepted(deviceConfig.HardwareId, true);
                    }
                }

                LoadBindings();
                
                var (success, error) = _interceptionService.Start();
                if (!success)
                {
                    MessageBox.Show($"Driver Error: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Select the first module by default
                if (ModulesListBox.Items.Count > 0)
                {
                    ModulesListBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Startup Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ModulesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModulesListBox.SelectedItem is Core.Interfaces.IPlugin plugin)
            {
                if (plugin.Id == "super_macros")
                {
                    SuperMacrosView.Visibility = Visibility.Visible;
                    PlaceholderView.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SuperMacrosView.Visibility = Visibility.Collapsed;
                    PlaceholderView.Visibility = Visibility.Visible;
                    PlaceholderTitle.Text = plugin.Name;
                }
            }
        }

        private void LoadBindings()
        {
            _bindings.Clear();
            foreach (var binding in _configManager.Config.KeyBindings)
            {
                var device = _devices.FirstOrDefault(d => d.HardwareId == binding.DeviceHardwareId);
                _bindings.Add(new BindingViewModel
                {
                    KeyName = binding.KeyName,
                    ScanCode = binding.ScanCode,
                    DeviceHardwareId = binding.DeviceHardwareId,
                    DeviceName = device?.FriendlyName ?? "Unknown Device",
                    ActionDescription = GetActionDescription(binding.Action),
                    Config = binding
                });
            }
        }

        private string GetActionDescription(ActionConfig action)
        {
            if (action == null) return "No action";
            return $"{action.TypeId}"; 
        }

        private void OnDeviceDetected(object sender, Core.Interception.KeyboardDevice device)
        {
            Dispatcher.Invoke(() =>
            {
                var existing = _devices.FirstOrDefault(d => d.HardwareId == device.HardwareId);
                if (existing == null)
                {
                    _devices.Add(new DeviceViewModel
                    {
                        HardwareId = device.HardwareId,
                        FriendlyName = device.FriendlyName ?? $"Keyboard {device.DeviceId}",
                        IsIntercepted = device.IsIntercepted,
                        DeviceId = device.DeviceId
                    });
                }
            });
        }

        private void OnKeyEvent(object sender, Core.Interception.KeyboardEventArgs e)
        {
            if (e.IsKeyDown)
            {
                Dispatcher.Invoke(() =>
                {
                    var deviceName = _devices.FirstOrDefault(d => d.HardwareId == e.Device?.HardwareId)?.FriendlyName ?? "Unknown";
                    TestAreaText.Text = $"{deviceName} : {e.KeyName}";
                });
            }

            if (!e.IsKeyDown) return;

            var binding = _configManager.Config.KeyBindings.FirstOrDefault(b =>
                b.DeviceHardwareId == e.Device?.HardwareId &&
                b.ScanCode == e.ScanCode &&
                b.TriggerOnKeyDown);

            if (binding != null)
            {
                if (binding.BlockKey)
                {
                    e.Handled = true;
                }

                var action = CreateActionFromConfig(binding.Action);
                if (action != null)
                {
                    _actionEngine.ExecuteAction(action, e);
                }
            }
        }

        private IAction CreateActionFromConfig(ActionConfig config)
        {
            if (config == null) return null;

            var action = _actionEngine.CreateAction(config.TypeId);
            if (action == null) return null;

            foreach (var param in config.Parameters)
            {
                var prop = action.GetType().GetProperty(param.Key);
                if (prop != null && prop.CanWrite)
                {
                    try
                    {
                        var value = Convert.ChangeType(param.Value, prop.PropertyType);
                        prop.SetValue(action, value);
                    }
                    catch { }
                }
            }

            return action;
        }

        private void DeviceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void BindingsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var hasSelection = BindingsListBox.SelectedItem != null;
            EditBindingButton.IsEnabled = hasSelection;
            DeleteBindingButton.IsEnabled = hasSelection;
        }

        private void BindingsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
             EditBindingButton_Click(sender, e);
        }

        private void DetectDeviceButton_Click(object sender, RoutedEventArgs e)
        {
             MessageBox.Show("Device detection dialog will be implemented here.", "Detect Device");
        }

        private void AddBindingButton_Click(object sender, RoutedEventArgs e)
        {
             MessageBox.Show("Binding editor will be implemented here.", "Add Binding");
        }

        private void EditBindingButton_Click(object sender, RoutedEventArgs e)
        {
             MessageBox.Show("Binding editor will be implemented here.", "Edit Binding");
        }

        private void DeleteBindingButton_Click(object sender, RoutedEventArgs e)
        {
            if (BindingsListBox.SelectedItem is BindingViewModel vm)
            {
                _configManager.Config.KeyBindings.Remove(vm.Config);
                _configManager.Save();
                LoadBindings();
            }
        }

        private void BuyPlugin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string url)
            {
                try
                {
                    System.Diagnostics.Process.Start(url);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open store: {ex.Message}");
                }
            }
        }
    }

    public class DeviceViewModel : INotifyPropertyChanged
    {
        public int DeviceId { get; set; }
        public string HardwareId { get; set; }
        
        private string _friendlyName;
        public string FriendlyName
        {
            get => _friendlyName;
            set { _friendlyName = value; OnPropertyChanged(nameof(FriendlyName)); }
        }

        private bool _isIntercepted;
        public bool IsIntercepted
        {
            get => _isIntercepted;
            set { _isIntercepted = value; OnPropertyChanged(nameof(IsIntercepted)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class BindingViewModel
    {
        public string KeyName { get; set; }
        public ushort ScanCode { get; set; }
        public string DeviceHardwareId { get; set; }
        public string DeviceName { get; set; }
        public string ActionDescription { get; set; }
        public KeyBindingConfig Config { get; set; }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public void Execute(object parameter) => _execute();
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
