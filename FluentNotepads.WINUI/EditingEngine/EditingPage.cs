using Microsoft.UI.Reactor;

namespace FluentNotepads.WINUI.EditingEngine;

/// <summary>
/// 编辑页面 - 使用官方 Microsoft.UI.Reactor 框架（0.1.0-preview.1）
/// 当前版本：黑色占位符
/// </summary>
public class EditingPage : Component
{
    public override Element Render()
    {
        // 返回一个黑色背景的 Border
        return Border(null)
            .Background("#000000");
    }
}
