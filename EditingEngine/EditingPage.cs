using Microsoft.UI.Reactor.Core;
using static Microsoft.UI.Reactor.Factories;

namespace FluentNotepads.EditingEngine;

/// <summary>
/// 编辑页面 - 使用 Reactor 框架
/// 共享于 UWP 和 WINUI 项目
/// 当前版本：简单文本测试
/// </summary>
public class EditingPage : Component
{
    public override Element Render()
    {
        // 简单显示文本
        return TextBlock("你好，纯代码世界！");
    }
}
