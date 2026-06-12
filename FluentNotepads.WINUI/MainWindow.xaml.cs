// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FluentNotepads_WINUI.Pages;
using FluentNotepads.EditingEngine;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Reactor.Hosting;
using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace FluentNotepads_WINUI
{
    public sealed partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }
        private int _tabCounter;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            if (Content is FrameworkElement root)
                root.RequestedTheme = AppThemeManager.CurrentTheme;

            // WinUI 3 设置标题栏（只设置一次）
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(CustomDragRegion);

            AppWindow.SetIcon("Assets/AppIcon.ico");

            // 添加键盘快捷键支持
            this.Content.KeyDown += OnKeyDown;

            if (Content is FrameworkElement rootEl)
                rootEl.Loaded += Root_Loaded;
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            if (Content is FrameworkElement root)
            {
                root.ActualThemeChanged -= AppThemeManager.OnActualThemeChanged;
                root.ActualThemeChanged += AppThemeManager.OnActualThemeChanged;
            }

            // 创建初始标签页（与 UWP 对齐）
            CreateNewTab("未命名");
        }

        /// <summary>
        /// 创建新标签页（与 UWP MainPage 的 CreateNewTab 对齐）
        /// </summary>
        private void CreateNewTab(string? fileName = null)
        {
            _tabCounter++;

            // 使用 ReactorHostControl 承载 Reactor 组件
            var reactorHost = new ReactorHostControl();
            reactorHost.Mount(new EditingPage());

            // 设置内容区背景（使用系统资源以支持半透明效果）
            var contentGrid = new Grid
            {
                Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["LayerFillColorDefaultBrush"]
            };
            contentGrid.Children.Add(reactorHost);

            // 创建标签页
            var header = fileName ?? $"未命名 {_tabCounter}";
            var newTab = new TabViewItem
            {
                Header = header,
                IconSource = new SymbolIconSource { Symbol = Symbol.Document },
                IsClosable = true,
                Content = contentGrid
            };

            TabView.TabItems.Add(newTab);
            TabView.SelectedItem = newTab;
        }

        private void CustomDragRegion_Loaded(object sender, RoutedEventArgs e)
        {
            // 监听窗口大小变化（对齐 UWP）
            AppWindow.Changed += AppWindow_Changed;
            UpdateTitleBarLayout();
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (args.DidSizeChange || args.DidPresenterChange)
            {
                UpdateTitleBarLayout();
            }
        }

        private void UpdateTitleBarLayout()
        {
            if (!AppWindowTitleBar.IsCustomizationSupported()) return;

            var titleBar = AppWindow.TitleBar;
            double scale = (Content as FrameworkElement)?.XamlRoot?.RasterizationScale ?? 1.0;
            if (scale <= 0) scale = 1.0;

            // 设置标题栏区域的高度和宽度
            double titleBarHeight = titleBar.Height / scale;
            ShellTitlebarInset.MinHeight = titleBarHeight;
            CustomDragRegion.MinHeight = titleBarHeight;

            double rightInset = titleBar.RightInset / scale;
            double leftInset = titleBar.LeftInset / scale;

            CustomDragRegion.MinWidth = rightInset;
            ShellTitlebarInset.MinWidth = leftInset;

            // 最大化时移除顶部 Padding（对齐 UWP）
            bool isMaximized = AppWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen ||
                               AppWindow.Presenter is OverlappedPresenter overlapped && overlapped.State == OverlappedPresenterState.Maximized;

            TabView.Padding = isMaximized
                ? new Thickness(0)
                : new Thickness(0, 8, 0, 0);
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Ctrl + , (逗号) 打开设置
            var ctrlState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
            if ((ctrlState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down)
            {
                // VirtualKey 188 是逗号键
                if ((int)e.Key == 188)
                {
                    OpenSettingsTab();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Creates a new tab with the given header and navigates it to the specified page type.
        /// </summary>
        private TabViewItem AddTab(string header, StorageFile? file = null)
        {
            // 使用 ReactorHostControl 并挂载 EditingPage 组件
            var reactorControl = new ReactorHostControl();
            reactorControl.Mount(new EditingPage());

            // 设置内容区背景（使用系统资源以支持半透明效果）
            var contentGrid = new Grid
            {
                Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["LayerFillColorDefaultBrush"]
            };
            contentGrid.Children.Add(reactorControl);

            var tab = new TabViewItem
            {
                Header = header,
                IconSource = new SymbolIconSource { Symbol = Symbol.Page },
                IsClosable = true,
                Content = contentGrid,
                Tag = file // 保存文件引用
            };

            TabView.TabItems.Add(tab);
            return tab;
        }

        /// <summary>
        /// 打开设置标签页
        /// </summary>
        public void OpenSettingsTab()
        {
            // 如果已有设置标签，直接切换到它
            foreach (var item in TabView.TabItems)
            {
                if (item is TabViewItem existing)
                {
                    if (existing.Content is Frame frame && frame.Content is SettingsPage)
                    {
                        TabView.SelectedItem = existing;
                        return;
                    }
                }
            }

            // 创建新的设置标签页
            var tab = new TabViewItem
            {
                Header = "设置",
                IconSource = new SymbolIconSource { Symbol = Symbol.Setting },
                IsClosable = true
            };

            var settingsFrame = new Frame();
            settingsFrame.Navigate(typeof(SettingsPage));
            tab.Content = settingsFrame;

            TabView.TabItems.Add(tab);
            TabView.SelectedItem = tab;
        }

        /// <summary>
        /// 从文件关联启动时打开文件
        /// </summary>
        public void OpenFileFromActivation(StorageFile file)
        {
            OpenFile(file);
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        private async void OpenFile(StorageFile file)
        {
            try
            {
                var text = await FileIO.ReadTextAsync(file);

                // 创建标签页
                var tab = AddTab(file.DisplayName, file);
                TabView.SelectedItem = tab;
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "错误",
                    Content = $"无法打开文件: {ex.Message}",
                    CloseButtonText = "确定",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// 拖拽文件进入窗口
        /// </summary>
        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.Caption = "打开文件";
                e.DragUIOverride.IsGlyphVisible = true;
                e.DragUIOverride.IsCaptionVisible = true;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }

        /// <summary>
        /// 拖拽文件放下
        /// </summary>
        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;

            var deferral = e.GetDeferral();
            try
            {
                var items = await e.DataView.GetStorageItemsAsync();
                var textFiles = items
                    .OfType<StorageFile>()
                    .Where(f => f.FileType.Equals(".txt", StringComparison.OrdinalIgnoreCase) ||
                               f.FileType.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
                               f.FileType.Equals(".log", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var file in textFiles)
                    OpenFile(file);
            }
            finally
            {
                deferral.Complete();
            }
        }

        /// <summary>
        /// Called when the user clicks the "+" button to add a new tab.
        /// </summary>
        private void TabView_AddTabButtonClick(TabView sender, object args)
        {
            CreateNewTab();
        }

        /// <summary>
        /// Called when a tab's close button is clicked.
        /// </summary>
        private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);

            // 如果没有标签页了，创建一个新的（与 UWP 对齐）
            if (sender.TabItems.Count == 0)
            {
                CreateNewTab("未命名");
            }
        }

        public void ApplySettings()
        {
            ElementSoundPlayer.State = SettingsManager.Instance.EnableSound
                ? ElementSoundPlayerState.On
                : ElementSoundPlayerState.Off;
        }
    }
}
