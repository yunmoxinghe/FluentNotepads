using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Core.V1Protocol;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using WinUI = Windows.UI.Xaml.Controls;
using MUXC = Microsoft.UI.Xaml.Controls;
using System;
using static Microsoft.UI.Reactor.Factories;

namespace FluentNotepads.EditingEngine;

/// <summary>
/// 编辑器工具栏组件
/// 参考 FluentPDF 的 UI 设计，提供现代化的 Fluent Design 风格工具栏
/// </summary>
public class EditorToolBar : Component
{
    // 回调事件
    public Action? OnNew { get; set; }
    public Action? OnOpen { get; set; }
    public Action? OnSave { get; set; }
    public Action? OnUndo { get; set; }
    public Action? OnRedo { get; set; }
    public Action? OnCut { get; set; }
    public Action? OnCopy { get; set; }
    public Action? OnPaste { get; set; }
    public Action? OnFind { get; set; }
    public Action? OnSettings { get; set; }
    
    // 状态属性
    public bool CanUndo { get; set; } = true;
    public bool CanRedo { get; set; } = true;

    public override Element Render()
    {
        return Grid(
            // 列定义：左自动、中填充、右自动
            columns: new[] { GridSize.Auto, GridSize.Star(), GridSize.Auto },
            rows: new[] { GridSize.Px(48), GridSize.Px(1) },  // 明确指定：48px工具栏 + 1px分割线
            
            // 左侧工具组 (Grid.Column=0)
            HStack(spacing: 4,
                // 文件操作组
                CreateAppBarButton("\uE8A5", "新建 (Ctrl+N)", OnNew),      // Document
                CreateAppBarButton("\uE8E5", "打开 (Ctrl+O)", OnOpen),      // OpenFile
                CreateAppBarButton("\uE74E", "保存 (Ctrl+S)", OnSave),      // Save

                // 分隔线
                CreateSeparator(),

                // 编辑操作组 - 根据状态显示/隐藏
                CanUndo ? CreateAppBarButton("\uE7A7", "撤销 (Ctrl+Z)", OnUndo) : null, // Undo
                CanRedo ? CreateAppBarButton("\uE7A6", "重做 (Ctrl+Y)", OnRedo) : null, // Redo
                
                (CanUndo || CanRedo) ? CreateSeparator() : null,

                // 剪贴板操作组
                CreateAppBarButton("\uE8C6", "剪切 (Ctrl+X)", OnCut),       // Cut
                CreateAppBarButton("\uE8C8", "复制 (Ctrl+C)", OnCopy),      // Copy
                CreateAppBarButton("\uE77F", "粘贴 (Ctrl+V)", OnPaste),     // Paste

                // 分隔线
                CreateSeparator(),

                // 查找
                CreateAppBarButton("\uE721", "查找 (Ctrl+F)", OnFind)       // Find
            )
            .Grid(row: 0, column: 0)
            .VAlign(VerticalAlignment.Center)
            .Margin(4, 0, 0, 0),

            // 右侧工具组 (Grid.Column=2)
            HStack(spacing: 4,
                // 设置按钮
                OnSettings != null ? CreateAppBarButton("\uE713", "设置", OnSettings) : null // Setting
            )
            .Grid(row: 0, column: 2)
            .VAlign(VerticalAlignment.Center)
            .Margin(0, 0, 4, 0),  // 右侧留出更多空间给标题栏按钮

            // 底部分割线 (Grid.Row=1, 跨越所有列) - 使用TabView相同的CardStrokeColorDefaultBrush
            Border(null)
                .Grid(row: 1, columnSpan: 3)
                .Set(border =>
                {
                    // 尝试多个可能的TabView边框颜色资源
                    if (Windows.UI.Xaml.Application.Current.Resources.TryGetValue(
                        "CardStrokeColorDefaultBrush", out var brush1) && brush1 is Brush b1)
                    {
                        border.Background = b1;
                    }
                    else if (Windows.UI.Xaml.Application.Current.Resources.TryGetValue(
                        "CardStrokeColorDefault", out var color) && color is Windows.UI.Color c)
                    {
                        border.Background = new SolidColorBrush(c);
                    }
                    else if (Windows.UI.Xaml.Application.Current.Resources.TryGetValue(
                        "DividerStrokeColorDefaultBrush", out var brush2) && brush2 is Brush b2)
                    {
                        border.Background = b2;
                    }
                    else
                    {
                        // 回退：浅灰色
                        border.Background = new SolidColorBrush(
                            Windows.UI.ColorHelper.FromArgb(20, 0, 0, 0));
                    }
                })
        )
        .Background(new SolidColorBrush(Windows.UI.Colors.Transparent));
    }

    /// <summary>
    /// 创建 AppBarButton（使用原生 AppBarButton 以正确支持深色/浅色主题）
    /// </summary>
    private Element CreateAppBarButton(string glyph, string tooltip, Action? onClick)
    {
        // 触发 Handler 注册
        _ = AppBarButtonReg.Done;
        
        return new AppBarButtonElement
        {
            Icon = new WinUI.FontIcon { Glyph = glyph },
            // LabelPosition 是 WinUI 3 特性，UWP 中不存在，移除此属性
            Width = 40,
            ToolTip = tooltip,
            OnClick = onClick != null ? _ => onClick() : null
        };
    }

    /// <summary>
    /// 创建视觉分隔符（使用TabView相同的分割线颜色）
    /// </summary>
    private Element CreateSeparator()
    {
        return Border(null)
            .Width(1)
            .Height(24)
            .Set(border =>
            {
                // 尝试多个可能的TabView边框颜色资源
                if (Windows.UI.Xaml.Application.Current.Resources.TryGetValue(
                    "CardStrokeColorDefaultBrush", out var brush1) && brush1 is Brush b1)
                {
                    border.Background = b1;
                }
                else if (Windows.UI.Xaml.Application.Current.Resources.TryGetValue(
                    "CardStrokeColorDefault", out var color) && color is Windows.UI.Color c)
                {
                    border.Background = new SolidColorBrush(c);
                }
                else if (Windows.UI.Xaml.Application.Current.Resources.TryGetValue(
                    "DividerStrokeColorDefaultBrush", out var brush2) && brush2 is Brush b2)
                {
                    border.Background = b2;
                }
                else
                {
                    // 回退
                    border.Background = new SolidColorBrush(
                        Windows.UI.ColorHelper.FromArgb(20, 0, 0, 0));
                }
            })
            .VAlign(VerticalAlignment.Center)
            .Margin(4, 0);
    }
}

// AppBarButton Element 定义
public record AppBarButtonElement : Element
{
    public object? Icon { get; init; }
    public double Width { get; init; }
    public string? ToolTip { get; init; }
    public Action<RoutedEventArgs>? OnClick { get; init; }
}

// AppBarButton Handler
public class AppBarButtonHandler : IElementHandler<AppBarButtonElement, WinUI.AppBarButton>
{
    public WinUI.AppBarButton Mount(MountContext ctx, AppBarButtonElement element)
    {
        var control = ctx.RentControl<WinUI.AppBarButton>();
        Update(default, element, element, control);
        return control;
    }

    public void Update(UpdateContext ctx, AppBarButtonElement oldEl, AppBarButtonElement newEl, WinUI.AppBarButton control)
    {
        control.Icon = newEl.Icon as WinUI.IconElement;
        control.Width = newEl.Width;
        
        if (!string.IsNullOrEmpty(newEl.ToolTip))
            WinUI.ToolTipService.SetToolTip(control, newEl.ToolTip);

        // 清除旧事件
        control.Click -= OnClick;
        
        // 添加新事件
        if (newEl.OnClick != null)
        {
            control.Click += OnClick;
        }

        void OnClick(object sender, RoutedEventArgs e) => newEl.OnClick?.Invoke(e);
    }
}

// 注册 shim
internal static class AppBarButtonReg
{
    static AppBarButtonReg() { }
    
    internal static readonly byte Done = Init();
    
    private static byte Init()
    {
        ControlRegistry.Register<AppBarButtonElement, WinUI.AppBarButton>(
            static () => new AppBarButtonHandler());
        return 1;
    }
}
