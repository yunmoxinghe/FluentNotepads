// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.Storage;

namespace FluentNotepads_WINUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Gets the main application window.
        /// </summary>
        public static Window? MainWindow { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            AppThemeManager.LoadSettings();

            MainWindow = new MainWindow();

            AppThemeManager.ApplyTheme(MainWindow);

            // 在激活前设置标题栏和材质，避免闪烁
            AppThemeManager.SetupTitleBar();
            AppThemeManager.ApplyMaterial();

            MainWindow.Activate();
        }
    }

    // ── 主题管理器 ─────────────────────────────────────────────────
    public static class AppThemeManager
    {
        public static ElementTheme CurrentTheme = ElementTheme.Default;
        public static BackgroundMaterial CurrentMaterial = BackgroundMaterial.Mica;

        public static void LoadSettings()
        {
            var s = SettingsManager.Instance;

            try
            {
                CurrentTheme = s.AppTheme switch
                {
                    "Light" => ElementTheme.Light,
                    "Dark" => ElementTheme.Dark,
                    _ => ElementTheme.Default
                };
            }
            catch { CurrentTheme = ElementTheme.Default; }

            try
            {
                CurrentMaterial = s.AppMaterial switch
                {
                    "MicaAlt" => BackgroundMaterial.MicaAlt,
                    "Acrylic" => BackgroundMaterial.Acrylic,
                    _ => BackgroundMaterial.Mica
                };
            }
            catch { CurrentMaterial = BackgroundMaterial.Mica; }

            try
            {
                ElementSoundPlayer.State = s.EnableSound
                    ? ElementSoundPlayerState.On
                    : ElementSoundPlayerState.Off;
            }
            catch { ElementSoundPlayer.State = ElementSoundPlayerState.On; }
        }

        public static void ApplyTheme(Window window)
        {
            try
            {
                if (window.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = CurrentTheme;
                    rootElement.ActualThemeChanged -= OnActualThemeChanged;
                    rootElement.ActualThemeChanged += OnActualThemeChanged;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApplyTheme failed: {ex.Message}");
            }
        }

        public static void ApplyMaterial()
        {
            if (App.MainWindow == null) return;
            try
            {
                // 检查是否已经是所需的材质，避免不必要的重新创建
                if (App.MainWindow.SystemBackdrop is Microsoft.UI.Xaml.Media.MicaBackdrop mica)
                {
                    if (CurrentMaterial == BackgroundMaterial.Mica && mica.Kind == Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base)
                        return;
                    if (CurrentMaterial == BackgroundMaterial.MicaAlt && mica.Kind == Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt)
                        return;
                }
                else if (App.MainWindow.SystemBackdrop is Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop &&
                         CurrentMaterial == BackgroundMaterial.Acrylic)
                {
                    return;
                }

                // 应用新材质
                App.MainWindow.SystemBackdrop = CurrentMaterial switch
                {
                    BackgroundMaterial.MicaAlt => new Microsoft.UI.Xaml.Media.MicaBackdrop 
                    { 
                        Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt 
                    },
                    BackgroundMaterial.Acrylic => new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop(),
                    _ => new Microsoft.UI.Xaml.Media.MicaBackdrop 
                    { 
                        Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base 
                    }
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApplyMaterial failed: {ex.Message}");
                App.MainWindow.SystemBackdrop = null;
            }
        }

        public static void SetupTitleBar()
        {
            if (App.MainWindow == null) return;
            try
            {
                if (!Microsoft.UI.Windowing.AppWindowTitleBar.IsCustomizationSupported())
                    return;

                var titleBar = App.MainWindow.AppWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                UpdateTitleBarColors();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SetupTitleBar failed: {ex.Message}");
            }
        }

        public static void UpdateTitleBarColors()
        {
            if (App.MainWindow == null) return;
            try
            {
                if (!Microsoft.UI.Windowing.AppWindowTitleBar.IsCustomizationSupported())
                    return;

                var titleBar = App.MainWindow.AppWindow.TitleBar;
                bool isDark = GetIsDarkTheme();

                var fg = isDark ? Colors.White : Colors.Black;
                var inactiveFg = isDark
                    ? Windows.UI.Color.FromArgb(255, 128, 128, 128)
                    : Windows.UI.Color.FromArgb(255, 160, 160, 160);
                var hoverBg = isDark
                    ? Windows.UI.Color.FromArgb(20, 255, 255, 255)
                    : Windows.UI.Color.FromArgb(20, 0, 0, 0);

                titleBar.ButtonForegroundColor = fg;
                titleBar.ButtonInactiveForegroundColor = inactiveFg;
                titleBar.ButtonHoverBackgroundColor = hoverBg;
                titleBar.ButtonHoverForegroundColor = fg;
                titleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(30, hoverBg.R, hoverBg.G, hoverBg.B);
                titleBar.ButtonPressedForegroundColor = fg;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateTitleBarColors failed: {ex.Message}");
            }
        }

        public static void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            UpdateTitleBarColors();
        }

        public static bool GetIsDarkTheme()
        {
            if (App.MainWindow?.Content is FrameworkElement rootElement)
            {
                var actual = rootElement.ActualTheme;
                if (actual != ElementTheme.Default)
                    return actual == ElementTheme.Dark;
            }

            return CurrentTheme == ElementTheme.Default
                ? Application.Current.RequestedTheme == ApplicationTheme.Dark
                : CurrentTheme == ElementTheme.Dark;
        }
    }

    public enum BackgroundMaterial { Mica, MicaAlt, Acrylic }

    // ── AOT 安全的 JSON 源生成上下文 ──────────────────────────────
    [JsonSerializable(typeof(AppSettings))]
    internal sealed partial class AppSettingsJsonContext : JsonSerializerContext { }

    // ── 设置管理器 ─────────────────────────────────────────────────
    public sealed class SettingsManager
    {
        private static SettingsManager? _instance;
        public static SettingsManager Instance => _instance ??= new SettingsManager();

        private readonly string _settingsFilePath;
        private AppSettings _settings;

        private SettingsManager()
        {
            _settingsFilePath = Path.Combine(
                ApplicationData.Current.LocalFolder.Path,
                "app_settings.json"
            );
            _settings = LoadSettingsFromFile();
        }

        public string AppTheme
        {
            get => _settings.AppTheme;
            set { _settings.AppTheme = value; SaveSettings(); }
        }

        public string AppMaterial
        {
            get => _settings.AppMaterial;
            set { _settings.AppMaterial = value; SaveSettings(); }
        }

        public bool EnableSound
        {
            get => _settings.EnableSound;
            set { _settings.EnableSound = value; SaveSettings(); }
        }

        private void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(_settings, AppSettingsJsonContext.Default.AppSettings);
                File.WriteAllText(_settingsFilePath, json);
                Debug.WriteLine($"[SettingsManager] 已保存: {_settingsFilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsManager] 保存失败: {ex.Message}");
            }
        }

        private AppSettings LoadSettingsFromFile()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings);
                    if (settings != null)
                    {
                        Debug.WriteLine("[SettingsManager] 从文件加载");
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsManager] 加载失败: {ex.Message}");
            }

            Debug.WriteLine("[SettingsManager] 使用默认设置");
            return new AppSettings();
        }
    }

    // ── 设置数据类（强类型，无装箱/拆箱，AOT 安全）─────────────────
    public sealed class AppSettings
    {
        public string AppTheme { get; set; } = "System";
        public string AppMaterial { get; set; } = "Mica";
        public bool EnableSound { get; set; } = false;
    }
}
