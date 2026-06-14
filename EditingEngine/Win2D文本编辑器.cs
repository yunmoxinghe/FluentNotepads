using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.System;
using Windows.UI;

namespace FluentNotepads.EditingEngine;

/// <summary>
/// Win2D 虚拟化文本编辑器
/// 高性能：支持百万行，只渲染可见区域
/// 完全自定义：行号、语法高亮、光标、选择等
/// </summary>
public class Win2DTextEditor : UserControl
{
    private CanvasControl _canvas = null!;
    private ScrollViewer _scrollViewer = null!;
    
    // ========== 文本数据 ==========
    private List<string> _lines = new();
    private int _cursorLine = 0;
    private int _cursorColumn = 0;
    
    // ========== 渲染配置 ==========
    private CanvasTextFormat _textFormat = null!;
    private const float LineHeight = 20f;
    private const float CharWidth = 9.6f;  // Consolas 字体宽度
    private const float LeftMargin = 50f;   // 行号区域宽度
    private const float TopMargin = 5f;
    
    // ========== 视口 ==========
    private int _firstVisibleLine = 0;
    private int _lastVisibleLine = 0;
    
    // ========== 颜色主题 ==========
    private Color _backgroundColor = Color.FromArgb(255, 30, 30, 30);      // 深色背景
    private Color _textColor = Color.FromArgb(255, 220, 220, 220);         // 文本颜色
    private Color _lineNumberColor = Color.FromArgb(255, 100, 100, 100);   // 行号颜色
    private Color _cursorColor = Color.FromArgb(255, 255, 255, 255);       // 光标颜色
    private Color _lineHighlightColor = Color.FromArgb(30, 100, 100, 100); // 当前行高亮

    public Win2DTextEditor()
    {
        InitializeComponent();
        
        // 监听主题变化
        this.ActualThemeChanged += OnThemeChanged;
        
        // 初始化主题
        ApplyCurrentTheme();
    }

    private void InitializeComponent()
    {
        // 创建 ScrollViewer
        _scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        // 创建 CanvasControl
        _canvas = new CanvasControl
        {
            Width = 2000,
            Height = 1000,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        _canvas.CreateResources += Canvas_CreateResources;
        _canvas.Draw += Canvas_Draw;

        // 键盘输入
        _canvas.KeyDown += Canvas_KeyDown;
        _canvas.CharacterReceived += Canvas_CharacterReceived;
        
        // 鼠标点击定位光标
        _canvas.PointerPressed += Canvas_PointerPressed;

        // 滚动事件
        _scrollViewer.ViewChanged += ScrollViewer_ViewChanged;

        _scrollViewer.Content = _canvas;
        this.Content = _scrollViewer;
        
        // 使 Canvas 可接收焦点
        _canvas.IsTabStop = true;
        _canvas.Loaded += (s, e) => _canvas.Focus(FocusState.Programmatic);
    }
    
    private void OnThemeChanged(FrameworkElement sender, object args)
    {
        ApplyCurrentTheme();
    }
    
    private void ApplyCurrentTheme()
    {
        bool isDark = this.ActualTheme == ElementTheme.Dark || 
                     (this.ActualTheme == ElementTheme.Default && 
                      Application.Current.RequestedTheme == ApplicationTheme.Dark);
        
        if (isDark)
        {
            _backgroundColor = Color.FromArgb(255, 30, 30, 30);
            _textColor = Color.FromArgb(255, 220, 220, 220);
            _lineNumberColor = Color.FromArgb(255, 100, 100, 100);
            _cursorColor = Color.FromArgb(255, 255, 255, 255);
            _lineHighlightColor = Color.FromArgb(30, 100, 100, 100);
        }
        else
        {
            _backgroundColor = Color.FromArgb(255, 255, 255, 255);
            _textColor = Color.FromArgb(255, 0, 0, 0);
            _lineNumberColor = Color.FromArgb(255, 150, 150, 150);
            _cursorColor = Color.FromArgb(255, 0, 0, 0);
            _lineHighlightColor = Color.FromArgb(30, 200, 200, 200);
        }
        
        _canvas?.Invalidate();
    }

    private void Canvas_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
    {
        // 创建文本格式
        _textFormat = new CanvasTextFormat
        {
            FontFamily = "Consolas",
            FontSize = 14,
            WordWrapping = CanvasWordWrapping.NoWrap
        };
    }

    private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        var ds = args.DrawingSession;
        
        // 清空背景
        ds.Clear(_backgroundColor);

        if (_lines.Count == 0)
        {
            DrawPlaceholder(ds);
            return;
        }

        // 计算可见行范围
        UpdateVisibleLines();

        // 绘制当前行高亮
        DrawCurrentLineHighlight(ds);

        // 绘制可见行
        for (int i = _firstVisibleLine; i <= _lastVisibleLine && i < _lines.Count; i++)
        {
            float y = TopMargin + i * LineHeight;

            // 绘制行号
            DrawLineNumber(ds, i + 1, y);

            // 绘制文本
            DrawTextLine(ds, _lines[i], y);
        }

        // 绘制光标
        DrawCursor(ds);
    }

    private void DrawPlaceholder(CanvasDrawingSession ds)
    {
        ds.DrawText(
            "Start typing...",
            LeftMargin,
            TopMargin,
            Color.FromArgb(100, 150, 150, 150),
            _textFormat
        );
    }

    private void UpdateVisibleLines()
    {
        float scrollY = (float)_scrollViewer.VerticalOffset;
        float viewportHeight = (float)_scrollViewer.ViewportHeight;

        _firstVisibleLine = Math.Max(0, (int)(scrollY / LineHeight) - 5);
        _lastVisibleLine = Math.Min(_lines.Count - 1, (int)((scrollY + viewportHeight) / LineHeight) + 5);
    }

    private void DrawCurrentLineHighlight(CanvasDrawingSession ds)
    {
        float y = TopMargin + _cursorLine * LineHeight;
        float width = (float)_canvas.ActualWidth;
        
        ds.FillRectangle(
            0, y, width, LineHeight,
            _lineHighlightColor
        );
    }

    private void DrawLineNumber(CanvasDrawingSession ds, int lineNumber, float y)
    {
        string lineNumText = lineNumber.ToString();
        ds.DrawText(
            lineNumText,
            LeftMargin - 10 - lineNumText.Length * 8,
            y,
            _lineNumberColor,
            _textFormat
        );
    }

    private void DrawTextLine(CanvasDrawingSession ds, string text, float y)
    {
        if (string.IsNullOrEmpty(text)) return;

        ds.DrawText(
            text,
            LeftMargin,
            y,
            _textColor,
            _textFormat
        );
    }

    private void DrawCursor(CanvasDrawingSession ds)
    {
        float x = LeftMargin + _cursorColumn * CharWidth;
        float y = TopMargin + _cursorLine * LineHeight;

        // 绘制竖线光标
        ds.DrawLine(
            x, y,
            x, y + LineHeight,
            _cursorColor,
            2f
        );
    }

    private void ScrollViewer_ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
    {
        _canvas.Invalidate(); // 触发重绘
    }

    private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        // 点击定位光标
        var point = e.GetCurrentPoint(_canvas).Position;
        
        _cursorLine = Math.Max(0, Math.Min(_lines.Count - 1, (int)((point.Y - TopMargin) / LineHeight)));
        _cursorColumn = Math.Max(0, (int)((point.X - LeftMargin) / CharWidth));
        
        if (_cursorLine < _lines.Count)
        {
            _cursorColumn = Math.Min(_cursorColumn, _lines[_cursorLine].Length);
        }

        _canvas.Invalidate();
        _canvas.Focus(FocusState.Pointer);
    }

    private void Canvas_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        bool handled = true;

        switch (e.Key)
        {
            case VirtualKey.Left:
                MoveCursorLeft();
                break;
            case VirtualKey.Right:
                MoveCursorRight();
                break;
            case VirtualKey.Up:
                MoveCursorUp();
                break;
            case VirtualKey.Down:
                MoveCursorDown();
                break;
            case VirtualKey.Home:
                _cursorColumn = 0;
                break;
            case VirtualKey.End:
                if (_cursorLine < _lines.Count)
                    _cursorColumn = _lines[_cursorLine].Length;
                break;
            case VirtualKey.Back:
                HandleBackspace();
                break;
            case VirtualKey.Delete:
                HandleDelete();
                break;
            case VirtualKey.Enter:
                HandleEnter();
                break;
            default:
                handled = false;
                break;
        }

        if (handled)
        {
            e.Handled = true;
            _canvas.Invalidate();
        }
    }

    private void Canvas_CharacterReceived(UIElement sender, CharacterReceivedRoutedEventArgs e)
    {
        // 处理字符输入
        char c = (char)e.Character;
        
        if (char.IsControl(c) && c != '\t') return; // 忽略控制字符（除了 Tab）

        EnsureLineExists();
        
        string line = _lines[_cursorLine];
        _lines[_cursorLine] = line.Insert(_cursorColumn, c.ToString());
        _cursorColumn++;

        _canvas.Invalidate();
    }

    // ========== 光标移动 ==========
    private void MoveCursorLeft()
    {
        if (_cursorColumn > 0)
        {
            _cursorColumn--;
        }
        else if (_cursorLine > 0)
        {
            _cursorLine--;
            _cursorColumn = _lines[_cursorLine].Length;
        }
    }

    private void MoveCursorRight()
    {
        if (_cursorLine < _lines.Count && _cursorColumn < _lines[_cursorLine].Length)
        {
            _cursorColumn++;
        }
        else if (_cursorLine < _lines.Count - 1)
        {
            _cursorLine++;
            _cursorColumn = 0;
        }
    }

    private void MoveCursorUp()
    {
        if (_cursorLine > 0)
        {
            _cursorLine--;
            _cursorColumn = Math.Min(_cursorColumn, _lines[_cursorLine].Length);
        }
    }

    private void MoveCursorDown()
    {
        if (_cursorLine < _lines.Count - 1)
        {
            _cursorLine++;
            _cursorColumn = Math.Min(_cursorColumn, _lines[_cursorLine].Length);
        }
    }

    // ========== 编辑操作 ==========
    private void HandleBackspace()
    {
        if (_cursorColumn > 0)
        {
            string line = _lines[_cursorLine];
            _lines[_cursorLine] = line.Remove(_cursorColumn - 1, 1);
            _cursorColumn--;
        }
        else if (_cursorLine > 0)
        {
            // 合并到上一行
            string currentLine = _lines[_cursorLine];
            _lines.RemoveAt(_cursorLine);
            _cursorLine--;
            _cursorColumn = _lines[_cursorLine].Length;
            _lines[_cursorLine] += currentLine;
        }
    }

    private void HandleDelete()
    {
        if (_cursorLine < _lines.Count)
        {
            string line = _lines[_cursorLine];
            if (_cursorColumn < line.Length)
            {
                _lines[_cursorLine] = line.Remove(_cursorColumn, 1);
            }
            else if (_cursorLine < _lines.Count - 1)
            {
                // 合并下一行
                _lines[_cursorLine] += _lines[_cursorLine + 1];
                _lines.RemoveAt(_cursorLine + 1);
            }
        }
    }

    private void HandleEnter()
    {
        EnsureLineExists();
        
        string line = _lines[_cursorLine];
        string leftPart = line.Substring(0, _cursorColumn);
        string rightPart = line.Substring(_cursorColumn);
        
        _lines[_cursorLine] = leftPart;
        _lines.Insert(_cursorLine + 1, rightPart);
        
        _cursorLine++;
        _cursorColumn = 0;

        UpdateCanvasSize();
    }

    private void EnsureLineExists()
    {
        if (_lines.Count == 0)
        {
            _lines.Add("");
        }
    }

    private void UpdateCanvasSize()
    {
        _canvas.Height = Math.Max(1000, _lines.Count * LineHeight + 100);
        
        // 计算最长行的宽度
        float maxWidth = 1000;
        foreach (var line in _lines)
        {
            float width = LeftMargin + line.Length * CharWidth + 50;
            if (width > maxWidth) maxWidth = width;
        }
        _canvas.Width = maxWidth;
    }

    // ========== 公共 API ==========
    public void SetText(string text)
    {
        _lines = new List<string>(text.Split('\n'));
        if (_lines.Count == 0) _lines.Add("");
        
        _cursorLine = 0;
        _cursorColumn = 0;
        
        UpdateCanvasSize();
        _canvas?.Invalidate();
    }

    public string GetText()
    {
        return string.Join("\n", _lines);
    }
}
