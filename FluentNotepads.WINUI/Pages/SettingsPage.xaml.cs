using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.ApplicationModel;

namespace FluentNotepads_WINUI.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private bool _isInitializing = true;
        private Window? _window;

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsPage_Loaded;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取当前窗口
            _window = App.MainWindow;

            LoadUI();
            LoadAppInfo();
            _isInitializing = false;

            // 注册主题变化事件
            if (_window?.Content is FrameworkElement root)
            {
                root.ActualThemeChanged -= OnActualThemeChanged;
                root.ActualThemeChanged += OnActualThemeChanged;
            }
        }

        private void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            // 主题变化时的处理
        }

        private void LoadUI()
        {
            var s = SettingsManager.Instance;

            RbTheme.SelectedIndex = s.AppTheme switch
            {
                "Light" => 1,
                "Dark" => 2,
                _ => 0
            };

            RbMaterial.SelectedIndex = s.AppMaterial switch
            {
                "MicaAlt" => 1,
                "Acrylic" => 2,
                _ => 0
            };

            SoundToggle.IsOn = s.EnableSound;
        }

        private void LoadAppInfo()
        {
            try
            {
                TxtAppName.Text = Package.Current.DisplayName;
                var v = Package.Current.Id.Version;
                TxtVersion.Text = $"版本 {v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
                TxtCopyright.Text = $"©{DateTime.Now.Year} {Package.Current.PublisherDisplayName}。保留所有权利。";
            }
            catch (Exception)
            {
                TxtAppName.Text = "FluentNotepads";
                TxtVersion.Text = "版本 1.0.0.0";
                TxtCopyright.Text = $"©{DateTime.Now.Year}。保留所有权利。";
            }
        }

        private async void OpenExternalLink(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton link && link.Tag is string url)
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
            }
        }

        private void RbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || _window == null) return;

            string value = RbTheme.SelectedIndex switch
            {
                1 => "Light",
                2 => "Dark",
                _ => "System"
            };

            SettingsManager.Instance.AppTheme = value;

            var theme = RbTheme.SelectedIndex switch
            {
                1 => ElementTheme.Light,
                2 => ElementTheme.Dark,
                _ => ElementTheme.Default
            };

            AppThemeManager.CurrentTheme = theme;
            AppThemeManager.ApplyTheme(_window);
        }

        private void RbMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            string value = RbMaterial.SelectedIndex switch
            {
                1 => "MicaAlt",
                2 => "Acrylic",
                _ => "Mica"
            };

            SettingsManager.Instance.AppMaterial = value;
            AppThemeManager.CurrentMaterial = value switch
            {
                "MicaAlt" => BackgroundMaterial.MicaAlt,
                "Acrylic" => BackgroundMaterial.Acrylic,
                _ => BackgroundMaterial.Mica
            };

            AppThemeManager.ApplyMaterial();
        }

        private void SoundToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            bool isOn = SoundToggle.IsOn;
            SettingsManager.Instance.EnableSound = isOn;

            ElementSoundPlayer.State = isOn
                ? ElementSoundPlayerState.On
                : ElementSoundPlayerState.Off;
        }
    }
}
