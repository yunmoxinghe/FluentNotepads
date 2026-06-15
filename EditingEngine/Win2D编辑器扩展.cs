namespace FluentNotepads.EditingEngine;

/// <summary>
/// Win2D 编辑器的流式扩展方法
/// 遵循 Reactor 的 fluent modifier 模式
/// </summary>
public static class Win2DEditorExtensions
{
    /// <summary>
    /// 设置编辑器的初始文本
    /// </summary>
    public static Win2DEditorElement Text(this Win2DEditorElement element, string text)
    {
        return element with { InitialText = text };
    }
}
