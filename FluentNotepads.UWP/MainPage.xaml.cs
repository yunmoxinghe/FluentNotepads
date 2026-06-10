using System;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MuxcTabView = Microsoft.UI.Xaml.Controls.TabView;
using MuxcTabViewItem = Microsoft.UI.Xaml.Controls.TabViewItem;
using MuxcTabViewTabCloseRequestedEventArgs = Microsoft.UI.Xaml.Controls.TabViewTabCloseRequestedEventArgs;
using MuxcSymbolIconSource = Microsoft.UI.Xaml.Controls.SymbolIconSource;
using FluentNotepads.UWP.EditingEngine;

namespace FluentNotepads
{
    /// <summary>
    /// 使用 WinUI 2 TabView 的多标签页记事本主页
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage? Instance { get; private set; }
        private int _newTabNumber = 1;

        public MainPage()
        {
            InitializeComponent();
            Instance = this;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 创建初始标签页
            CreateNewTab("未命名");
        }

        private void MainPage_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            // Ctrl + , (逗号) 打开设置
            var ctrlState = Windows.UI.Core.CoreWindow.GetForCurrentThread().GetKeyState(Windows.System.VirtualKey.Control);
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

        private void CustomDragRegion_Loaded(object sender, RoutedEventArgs e)
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            UpdateTitleBarInsets(coreTitleBar);
            Window.Current.SetTitleBar(CustomDragRegion);

            // 监听窗口大小变化
            Window.Current.SizeChanged += Window_SizeChanged;
        }

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            UpdateTitleBarInsets(coreTitleBar);
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
            => UpdateTitleBarInsets(sender);

        private void UpdateTitleBarInsets(CoreApplicationViewTitleBar coreTitleBar)
        {
            ShellTitlebarInset.MinHeight = coreTitleBar.Height;
            CustomDragRegion.MinHeight = coreTitleBar.Height;

            if (FlowDirection == FlowDirection.LeftToRight)
            {
                CustomDragRegion.MinWidth = coreTitleBar.SystemOverlayRightInset;
                ShellTitlebarInset.MinWidth = coreTitleBar.SystemOverlayLeftInset;
            }
            else
            {
                CustomDragRegion.MinWidth = coreTitleBar.SystemOverlayLeftInset;
                ShellTitlebarInset.MinWidth = coreTitleBar.SystemOverlayRightInset;
            }

            // 最大化时移除顶部 Padding
            var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            bool isMaximized = appView.IsFullScreenMode || IsWindowMaximized();
            TabView.Padding = isMaximized
                ? new Thickness(0)
                : new Thickness(0, 8, 0, 0);
        }

        private static bool IsWindowMaximized()
        {
            var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            return appView.AdjacentToLeftDisplayEdge && appView.AdjacentToRightDisplayEdge;
        }

        /// <summary>
        /// 创建新标签页
        /// </summary>
        private void CreateNewTab(string? fileName = null)
        {
            _newTabNumber++;

            // 创建 Reactor 宿主容器
            var container = new Grid();
            var reactorHost = new Microsoft.UI.Reactor.ReactorHost(container);
            
            // 渲染 Reactor 编辑页面组件
            reactorHost.Render(new EditingPage());

            // 创建标签页
            var header = fileName ?? $"未命名 {_newTabNumber}";
            var newTab = new MuxcTabViewItem
            {
                Header = header,
                IconSource = new MuxcSymbolIconSource { Symbol = Symbol.Document },
                IsClosable = true,
                Content = container  // 使用容器作为内容
            };

            TabView.TabItems.Add(newTab);
            TabView.SelectedItem = newTab;
        }

        /// <summary>
        /// 添加新标签页按钮点击
        /// </summary>
        private void TabView_AddTabButtonClick(MuxcTabView sender, object args)
        {
            CreateNewTab();
        }

        /// <summary>
        /// 关闭标签页
        /// </summary>
        private void TabView_TabCloseRequested(MuxcTabView sender, MuxcTabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);

            // 如果没有标签页了，创建一个新的
            if (sender.TabItems.Count == 0)
            {
                CreateNewTab("未命名");
            }
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

                // 创建 Reactor 宿主容器
                var container = new Grid();
                var reactorHost = new Microsoft.UI.Reactor.ReactorHost(container);
                
                // 渲染 Reactor 编辑页面组件
                reactorHost.Render(new EditingPage());
                // TODO: 将文本传递给 EditingPage

                // 创建标签页
                var newTab = new MuxcTabViewItem
                {
                    Header = file.DisplayName,
                    IconSource = new MuxcSymbolIconSource { Symbol = Symbol.Page },
                    IsClosable = true,
                    Content = container,  // 使用容器作为内容
                    Tag = file // 保存文件引用
                };

                TabView.TabItems.Add(newTab);
                TabView.SelectedItem = newTab;
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "错误",
                    Content = $"无法打开文件: {ex.Message}",
                    CloseButtonText = "确定"
                };
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// 拖拽文件进入窗口
        /// </summary>
        private void MainPage_DragOver(object sender, DragEventArgs e)
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
        private async void MainPage_Drop(object sender, DragEventArgs e)
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
        /// 打开设置标签页
        /// </summary>
        public void OpenSettingsTab()
        {
            // 如果已有设置标签，直接切换到它
            foreach (var item in TabView.TabItems)
            {
                if (item is MuxcTabViewItem existing && existing.Content is Pages.SettingsPage)
                {
                    TabView.SelectedItem = existing;
                    return;
                }
            }

            // 创建新的设置标签页
            var tab = new MuxcTabViewItem
            {
                Header = "设置",
                IconSource = new MuxcSymbolIconSource { Symbol = Symbol.Setting },
                IsClosable = true,
                Content = new Pages.SettingsPage()
            };
            TabView.TabItems.Add(tab);
            TabView.SelectedItem = tab;
        }

        public async void OpenExternalLink(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton link && link.Tag is string url)
            {
                // TODO: 添加外部链接打开确认对话框
                await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
            }
        }

        public void ApplySettings()
        {
            ElementSoundPlayer.State = SettingsManager.Instance.EnableSound
                ? ElementSoundPlayerState.On
                : ElementSoundPlayerState.Off;

            // TODO: 应用其他设置到所有打开的编辑器
        }
    }
}
