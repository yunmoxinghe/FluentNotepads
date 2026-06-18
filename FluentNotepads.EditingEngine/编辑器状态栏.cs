using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System;
using static Microsoft.UI.Reactor.Factories;

namespace FluentNotepads.EditingEngine;

/// <summary>
/// 编辑器状态栏组件 - 像素级参考 Notepads 原版布局
/// 显示光标位置、文件信息、编码等状态信息
/// 每个组件都可点击交互，具有 Hover 效果
/// </summary>
public class EditorStatusBar : Component
{
    // 状态信息属性
    public int Line { get; set; } = 1;
    public int Column { get; set; } = 1;
    public int TotalLines { get; set; } = 1;
    public string Encoding { get; set; } = "UTF-8";
    public string LineEnding { get; set; } = "CRLF";
    public bool IsModified { get; set; } = false;
    public bool IsFileModifiedExternally { get; set; } = false;
    public string FilePath { get; set; } = "";
    public long FileSize { get; set; } = 0;
    public int FontZoomLevel { get; set; } = 100;

    // 回调事件
    public Action? OnFileStatusClick { get; set; }
    public Action? OnFilePathClick { get; set; }
    public Action? OnModificationClick { get; set; }
    public Action? OnLineColumnClick { get; set; }
    public Action? OnFontZoomClick { get; set; }
    public Action? OnLineEndingClick { get; set; }
    public Action? OnEncodingClick { get; set; }

    public override Element Render()
    {
        return Grid(
            columns: new[]
            {
                GridSize.Auto,      // 0. 文件路径（自适应宽度）
                GridSize.Auto,      // 1. 修改指示器
                GridSize.Star(1),   // 2. 占位符（占据剩余空间）
                GridSize.Auto,      // 3. 行/列位置
                GridSize.Px(1),     // 4. 分割线
                GridSize.Auto,      // 5. 字体缩放
                GridSize.Px(1),     // 6. 分割线
                GridSize.Auto,      // 7. 行尾符
                GridSize.Px(1),     // 8. 分割线
                GridSize.Auto,      // 9. 编码
            },
            rows: new[] { GridSize.Px(32) },  // 固定32px高度
            
            // 1. 文件路径（左侧，自适应宽度）
            CreatePathIndicator()
                .Grid(column: 0),

            // 2. 修改指示器
            CreateModificationIndicator()
                .Grid(column: 1),

            // 3. 占位符（占据剩余空间，把右侧按钮推到最右边）
            Border(TextBlock(""))
                .Grid(column: 2),

            // 4. 行列指示器
            CreateStatusBarButton($"行 {Line}，列 {Column}", OnLineColumnClick)
                .Grid(column: 3),

            // 5. 垂直分割线
            CreateVerticalSeparator()
                .Grid(column: 4),

            // 6. 字体缩放
            CreateStatusBarButton($"{FontZoomLevel}%", OnFontZoomClick)
                .Grid(column: 5),

            // 7. 垂直分割线
            CreateVerticalSeparator()
                .Grid(column: 6),

            // 8. 行尾符
            CreateStatusBarButton(LineEnding, OnLineEndingClick)
                .Grid(column: 7),

            // 9. 垂直分割线
            CreateVerticalSeparator()
                .Grid(column: 8),

            // 10. 编码
            CreateStatusBarButton(Encoding, OnEncodingClick)
                .Grid(column: 9)
        )
        .Set(g =>
        {
            g.BorderThickness = new Thickness(0, 1, 0, 0);  // 只有顶部 1px 边框
            
            // 尝试多个可能的TabView边框颜色资源
            if (Windows.UI.Xaml.Application.Current.Resources.TryGetValue(
                "CardStrokeColorDefaultBrush", out var brush1) && brush1 is Brush b1)
            {
                g.BorderBrush = b1;
            }
            else if (Windows.UI.Xaml.Application.Current.Resources.TryGetValue(
                "CardStrokeColorDefault", out var color) && color is Windows.UI.Color c)
            {
                g.BorderBrush = new SolidColorBrush(c);
            }
            else if (Windows.UI.Xaml.Application.Current.Resources.TryGetValue(
                "DividerStrokeColorDefaultBrush", out var brush2) && brush2 is Brush b2)
            {
                g.BorderBrush = b2;
            }
            else
            {
                // 回退
                g.BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(20, 0, 0, 0));
            }
        });
    }

    /// <summary>
    /// 创建垂直分割线 - 上下留 6px 间隙
    /// </summary>
    private Element CreateVerticalSeparator()
    {
        return Border(null)
            .Width(1)
            .Height(20)  // 高度 20px = 32px 状态栏高度 - 上下各 6px 间隙
            .VerticalAlignment(VerticalAlignment.Center)
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
                        ColorHelper.FromArgb(20, 0, 0, 0));
                }
            });
    }

    /// <summary>
    /// 创建文件路径指示器 - 自适应文本宽度
    /// </summary>
    private Element CreatePathIndicator()
    {
        var fileName = string.IsNullOrEmpty(FilePath) 
            ? "未命名" 
            : System.IO.Path.GetFileName(FilePath);

        return Button(
            TextBlock(fileName)
                .FontSize(12)
                .Foreground("#A0A0A0")
                .Set(tb =>
                {
                    tb.TextTrimming = TextTrimming.CharacterEllipsis;
                    tb.VerticalAlignment = VerticalAlignment.Center;
                })
        )
        .SubtleButton()  // 使用ReactorUWP的SubtleButton扩展方法
        .Set(btn =>
        {
            btn.Padding = new Thickness(12, 0, 12, 0);
            btn.Height = 32;
            btn.MinWidth = 0;
            btn.HorizontalAlignment = HorizontalAlignment.Left;
            btn.VerticalAlignment = VerticalAlignment.Stretch;
            btn.HorizontalContentAlignment = HorizontalAlignment.Left;
            btn.VerticalContentAlignment = VerticalAlignment.Center;
            
            if (OnFilePathClick != null)
            {
                btn.Click += (s, e) => OnFilePathClick();
            }
        });
    }

    /// <summary>
    /// 创建修改指示器 - 显示"已修改"文本
    /// </summary>
    private Element CreateModificationIndicator()
    {
        if (!IsModified)
        {
            return Border(TextBlock(""))
                .Width(0)
                .Height(32);
        }

        return Button(
            TextBlock("●  已修改")
                .FontSize(12)
                .Foreground("#0078D4")
                .Set(tb =>
                {
                    tb.VerticalAlignment = VerticalAlignment.Center;
                })
        )
        .SubtleButton()  // 使用ReactorUWP的SubtleButton扩展方法
        .Set(btn =>
        {
            btn.Padding = new Thickness(12, 0, 12, 0);
            btn.Height = 32;
            btn.MinWidth = 0;
            btn.HorizontalAlignment = HorizontalAlignment.Stretch;
            btn.VerticalAlignment = VerticalAlignment.Stretch;
            btn.HorizontalContentAlignment = HorizontalAlignment.Center;
            btn.VerticalContentAlignment = VerticalAlignment.Center;
            
            if (OnModificationClick != null)
            {
                btn.Click += (s, e) => OnModificationClick();
            }
        });
    }

    /// <summary>
    /// 创建通用状态栏按钮 - 使用ReactorUWP的SubtleButton扩展
    /// </summary>
    private Element CreateStatusBarButton(string text, Action? onClick)
    {
        return Button(
            TextBlock(text)
                .FontSize(12)
                .Foreground("#A0A0A0")
                .Set(tb =>
                {
                    tb.VerticalAlignment = VerticalAlignment.Center;
                })
        )
        .SubtleButton()  // 使用ReactorUWP的SubtleButton扩展方法
        .Set(btn =>
        {
            btn.Padding = new Thickness(12, 0, 12, 0);
            btn.Height = 32;
            btn.MinWidth = 0;
            btn.HorizontalAlignment = HorizontalAlignment.Stretch;
            btn.VerticalAlignment = VerticalAlignment.Stretch;
            btn.HorizontalContentAlignment = HorizontalAlignment.Center;
            btn.VerticalContentAlignment = VerticalAlignment.Center;
            
            if (onClick != null)
            {
                btn.Click += (s, e) => onClick();
            }
        });
    }

    /// <summary>
    /// 格式化文件大小
    /// </summary>
    private string FormatFileSize(long bytes)
    {
        if (bytes == 0) return "0 B";
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
        return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
    }
}
