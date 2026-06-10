using Microsoft.UI.Reactor;
using static Microsoft.UI.Reactor.BorderFactory;

namespace FluentNotepads.UWP.EditingEngine;

/// <summary>
/// 编辑页面 - 纯 Reactor 组件
/// 当前版本：黑色占位符
/// </summary>
public class EditingPage : Component
{
    public override Element Render()
    {
        // 返回一个黑色背景的 Border，填充整个区域
        return Border(new EmptyElement())
            .Background("#000000");
    }
}
