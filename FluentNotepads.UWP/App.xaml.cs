using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace FluentNotepads
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default <see cref="Application"/> class.
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            
            // 添加全局异常处理，帮助定位问题
            UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            try
            {
                var ex = e.Exception;
                var msg = $@"
========== 未处理的异常 ==========
类型: {ex?.GetType().FullName}
消息: {ex?.Message}
堆栈跟踪:
{ex?.StackTrace}

内部异常: {ex?.InnerException?.Message}
内部堆栈:
{ex?.InnerException?.StackTrace}
================================";
                
                Debug.WriteLine(msg);
                System.Diagnostics.Debugger.Break(); // 触发调试器中断
                
                e.Handled = true; // 阻止应用崩溃
            }
            catch (Exception ex2)
            {
                Debug.WriteLine($"异常处理器本身出错: {ex2}");
            }
        }

        /// <inheritdoc/>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active.
            if (Window.Current.Content is not Frame rootFrame)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                AppThemeManager.LoadSettings();

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
                
                // 应用主题和材质
                AppThemeManager.ApplyTheme();
                AppThemeManager.ApplyMaterial();
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page, configuring
                    // the new page by passing required information as a navigation parameter.
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// 处理文件关联启动
        /// </summary>
        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;

                AppThemeManager.LoadSettings();
                AppThemeManager.ApplyTheme();
                AppThemeManager.ApplyMaterial();
            }

            if (rootFrame.Content == null)
                rootFrame.Navigate(typeof(MainPage), "fileActivated");

            Window.Current.Activate();

            // 打开文件
            var file = args.Files.Count > 0 ? args.Files[0] as StorageFile : null;
            if (file == null) return;

            if (rootFrame.Content is MainPage ready)
            {
                ready.OpenFileFromActivation(file);
            }
            else
            {
                void OnNavigated(object s, NavigationEventArgs e)
                {
                    rootFrame.Navigated -= OnNavigated;
                    if (rootFrame.Content is MainPage mainPage)
                        mainPage.OpenFileFromActivation(file);
                }
                rootFrame.Navigated += OnNavigated;
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails.
        /// </summary>
        /// <param name="sender">The Frame which failed navigation.</param>
        /// <param name="e">Details about the navigation failure.</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load page '{e.SourcePageType.FullName}'.");
        }

        /// <summary>
        /// Invoked when application execution is being suspended. Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
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
                CurrentMaterial = s.AppMaterial == "Acrylic"
                    ? BackgroundMaterial.Acrylic
                    : BackgroundMaterial.Mica;
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

        public static void ApplyTheme()
        {
            try
            {
                if (Window.Current.Content is FrameworkElement rootElement)
                    rootElement.RequestedTheme = CurrentTheme;

                CustomizeTitleBar();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApplyTheme failed: {ex.Message}");
            }
        }

        public static void ApplyMaterial()
        {
            try
            {
                var rootFrame = Window.Current.Content as Frame;
                if (rootFrame == null) return;

                if (CurrentMaterial == BackgroundMaterial.Mica)
                {
                    if (rootFrame is FrameworkElement el)
                        el.ActualThemeChanged -= OnActualThemeChanged;

                    rootFrame.Background = null;
                    Microsoft.UI.Xaml.Controls.BackdropMaterial.SetApplyToRootOrPageBackground(rootFrame, true);
                }
                else
                {
                    Microsoft.UI.Xaml.Controls.BackdropMaterial.SetApplyToRootOrPageBackground(rootFrame, false);

                    var isDark = GetIsDarkTheme();
                    var tintColor = isDark
                        ? Color.FromArgb(255, 32, 32, 32)
                        : Color.FromArgb(255, 243, 243, 243);

                    rootFrame.Background = new AcrylicBrush
                    {
                        BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                        TintColor = tintColor,
                        TintOpacity = 0.8,
                        FallbackColor = tintColor
                    };

                    if (rootFrame is FrameworkElement el)
                    {
                        el.ActualThemeChanged -= OnActualThemeChanged;
                        el.ActualThemeChanged += OnActualThemeChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApplyMaterial failed: {ex.Message}");
            }
        }

        public static void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            CustomizeTitleBar();

            if (CurrentMaterial == BackgroundMaterial.Acrylic)
            {
                var rootFrame = Window.Current.Content as Frame;
                if (rootFrame == null) return;

                var isDark = GetIsDarkTheme();
                var tintColor = isDark
                    ? Color.FromArgb(255, 32, 32, 32)
                    : Color.FromArgb(255, 243, 243, 243);

                if (rootFrame.Background is AcrylicBrush brush)
                {
                    brush.TintColor = tintColor;
                    brush.FallbackColor = tintColor;
                }
            }
        }

        public static bool GetIsDarkTheme()
        {
            if (Window.Current?.Content is FrameworkElement rootElement)
            {
                var actual = rootElement.ActualTheme;
                if (actual != ElementTheme.Default)
                    return actual == ElementTheme.Dark;
            }
            if (CurrentTheme == ElementTheme.Default)
                return Application.Current.RequestedTheme == ApplicationTheme.Dark;
            return CurrentTheme == ElementTheme.Dark;
        }

        public static void CustomizeTitleBar()
        {
            try
            {
                var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.ExtendViewIntoTitleBar = true;

                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                var isDark = GetIsDarkTheme();
                var fg = isDark ? Colors.White : Colors.Black;
                var inactiveFg = isDark
                    ? Color.FromArgb(255, 128, 128, 128)
                    : Color.FromArgb(255, 160, 160, 160);
                var hoverBg = isDark
                    ? Color.FromArgb(20, 255, 255, 255)
                    : Color.FromArgb(20, 0, 0, 0);

                titleBar.ButtonForegroundColor = fg;
                titleBar.ButtonInactiveForegroundColor = inactiveFg;
                titleBar.ButtonHoverBackgroundColor = hoverBg;
                titleBar.ButtonHoverForegroundColor = fg;
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(30, hoverBg.R, hoverBg.G, hoverBg.B);
                titleBar.ButtonPressedForegroundColor = fg;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CustomizeTitleBar failed: {ex.Message}");
            }
        }
    }

    public enum BackgroundMaterial { Mica, Acrylic }

    [JsonSerializable(typeof(AppSettings))]
    internal sealed partial class AppSettingsJsonContext : JsonSerializerContext { }

    public sealed class SettingsManager
    {
        private static SettingsManager? _instance;
        public static SettingsManager Instance => _instance ??= new SettingsManager();

        private readonly string _settingsFilePath;
        private AppSettings _settings;

        private SettingsManager()
        {
            _settingsFilePath = Path.Combine(
                ApplicationData.Current.LocalFolder.Path, "app_settings.json");
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
            string json;
            try
            {
                json = JsonSerializer.Serialize(_settings, AppSettingsJsonContext.Default.AppSettings);
            }
            catch (Exception ex) { Debug.WriteLine($"[SettingsManager] 序列化失败: {ex.Message}"); return; }

            var path = _settingsFilePath;
            _ = System.Threading.Tasks.Task.Run(() =>
            {
                try { File.WriteAllText(path, json); }
                catch (Exception ex) { Debug.WriteLine($"[SettingsManager] 保存失败: {ex.Message}"); }
            });
        }

        private AppSettings LoadSettingsFromFile()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var s = JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings);
                    if (s != null) return s;
                }
            }
            catch (Exception ex) { Debug.WriteLine($"[SettingsManager] 加载失败: {ex.Message}"); }
            return new AppSettings();
        }
    }

    public sealed class AppSettings
    {
        public string AppTheme { get; set; } = "System";
        public string AppMaterial { get; set; } = "Mica";
        public bool EnableSound { get; set; } = false;
    }
}
