using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Core.V1Protocol;
using Microsoft.UI.Xaml;

namespace FluentNotepads.EditingEngine;

/// <summary>
/// Reactor 元素处理器 - 处理 Win2DEditorElement 的挂载和更新
/// </summary>
public class Win2DEditorElementHandler : IElementHandler<Win2DEditorElement, Win2DTextEditor>
{
    public Win2DTextEditor Mount(MountContext ctx, Win2DEditorElement element)
    {
        var editor = new Win2DTextEditor();
        editor.SetText(element.InitialText);
        return editor;
    }

    public void Update(UpdateContext ctx, Win2DEditorElement oldEl, Win2DEditorElement newEl, Win2DTextEditor control)
    {
        // 如果文本改变了，更新编辑器内容
        if (oldEl.InitialText != newEl.InitialText)
        {
            control.SetText(newEl.InitialText);
        }
    }

    public void Unmount(UnmountContext ctx, Win2DTextEditor control)
    {
        // 清理资源（如果需要）
    }
}
