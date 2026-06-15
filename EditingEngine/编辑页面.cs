using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Core.V1Protocol;
using Microsoft.UI.Xaml.Controls;  // InfoBarSeverity
using static Microsoft.UI.Reactor.Factories;
using static FluentNotepads.EditingEngine.EditingEngineFactories;

namespace FluentNotepads.EditingEngine;

/// <summary>
/// 编辑页面 - 使用 Reactor 框架
/// 共享于 UWP 和 WINUI 项目
/// 当前版本：Win2D 虚拟化高性能编辑器
/// </summary>
public class EditingPage : Component
{
    // 外部回调
    public Action? OnSettingsRequested { get; set; }
    
    private string _editorText = @"Welcome to FluentNotepads Win2D Editor!

🚀 Features:
- Virtualized rendering (支持百万行)
- Only visible lines are rendered
- Smooth scrolling
- Full keyboard support
- Mouse click positioning

Try editing this text!
Type something new...

Performance test: 这个编辑器可以轻松处理超大文件
- 10MB: ✅ Instant
- 100MB: ✅ Instant  
- 1GB: ✅ Still fast!

技术栈:
- Win2D (DirectX 渲染)
- 虚拟化视口
- 自定义光标和选择
- 行号显示
- 深色主题";

    // 状态
    private bool _isModified = false;

    public override Element Render()
    {
        var (line, setLine) = UseState(1);
        var (column, setColumn) = UseState(1);
        
        return Grid(
            columns: new[] { GridSize.Star(1) },
            rows: new[] 
            { 
                GridSize.Star(1),   // 主内容区 (工具栏 + 编辑器)
                GridSize.Auto       // 状态栏
            },
            
            // 主内容区 - 工具栏 + 编辑器
            Grid(
                columns: new[] { GridSize.Star(1) },
                rows: new[]
                {
                    GridSize.Auto,      // 工具栏
                    GridSize.Star(1)    // 编辑器 - 占据剩余空间
                },

                // 工具栏
                new EditorToolBar
                {
                    OnNew = () => ShowNotImplemented("新建"),
                    OnOpen = () => ShowNotImplemented("打开"),
                    OnSave = () => ShowNotImplemented("保存"),
                    OnUndo = () => ShowNotImplemented("撤销"),
                    OnRedo = () => ShowNotImplemented("重做"),
                    OnCut = () => ShowNotImplemented("剪切"),
                    OnCopy = () => ShowNotImplemented("复制"),
                    OnPaste = () => ShowNotImplemented("粘贴"),
                    OnFind = () => ShowNotImplemented("查找"),
                    OnSettings = () => OnSettingsRequested?.Invoke()
                }.Render()
                    .Grid(row: 0),

                // Win2D 编辑器 - 占据剩余空间
                Win2DEditor(_editorText)
                    .Grid(row: 1)
            )
            .Grid(row: 0),

            // 状态栏 - 参考 Notepads 布局
            new EditorStatusBar
            {
                Line = line,
                Column = column,
                TotalLines = _editorText.Split('\n').Length,
                IsModified = _isModified,
                FilePath = "",
                FileSize = System.Text.Encoding.UTF8.GetByteCount(_editorText),
                Encoding = "UTF-8",
                LineEnding = "CRLF",
                FontZoomLevel = 100,
                IsFileModifiedExternally = false,
                OnFileStatusClick = () => ShowNotImplemented("文件状态"),
                OnFilePathClick = () => ShowNotImplemented("文件路径"),
                OnModificationClick = () => ShowNotImplemented("修改状态"),
                OnLineColumnClick = () => ShowNotImplemented("跳转到行"),
                OnFontZoomClick = () => ShowNotImplemented("字体缩放"),
                OnLineEndingClick = () => ShowNotImplemented("更改行尾符"),
                OnEncodingClick = () => ShowNotImplemented("更改编码")
            }.Render()
                .Grid(row: 1)
        );
    }
    
    private void ShowNotImplemented(string feature)
    {
        // TODO: 显示提示消息
        System.Diagnostics.Debug.WriteLine($"功能待实现: {feature}");
    }
    
    // 工厂方法
    private static Win2DEditorElement Win2DEditor(string initialText)
    {
        // 触发静态注册
        _ = Win2DEditorReg.Done;
        return new Win2DEditorElement(initialText);
    }
}

/// <summary>
/// Win2D 编辑器的 Reactor 元素包装
/// </summary>
public record Win2DEditorElement(string InitialText) : Element
{
}

// 注册 shim - 使用标准 Reactor ControlRegistry.Register 模式
internal static class Win2DEditorReg
{
    static Win2DEditorReg() { }
    
    internal static readonly byte Done = Init();
    
    private static byte Init()
    {
        ControlRegistry.Register<Win2DEditorElement, Win2DTextEditor>(
            static () => new Win2DEditorDescriptorHandler());
        return 1;
    }
}
