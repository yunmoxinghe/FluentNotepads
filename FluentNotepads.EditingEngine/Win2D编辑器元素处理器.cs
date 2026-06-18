using Microsoft.UI.Reactor.Core.V1Protocol;
using Microsoft.UI.Reactor.Core.V1Protocol.Descriptor;

namespace FluentNotepads.EditingEngine;

/// <summary>
/// Win2D 编辑器的标准 ControlDescriptor
/// 使用 Reactor V1 Protocol 声明式模式
/// </summary>
internal static class Win2DEditorDescriptor
{
    internal static readonly ControlDescriptor<Win2DEditorElement, Win2DTextEditor> Descriptor =
        new ControlDescriptor<Win2DEditorElement, Win2DTextEditor>()
            // 属性：InitialText → SetText (单向绑定)
            .OneWay(
                get: static e => e.InitialText,
                set: static (c, v) => c.SetText(v));
}

/// <summary>
/// Win2D 编辑器的 Handler 实现
/// 用于 Reactor V1 Reg 注册模式
/// </summary>
internal sealed class Win2DEditorDescriptorHandler() 
    : DescriptorHandler<Win2DEditorElement, Win2DTextEditor>(Win2DEditorDescriptor.Descriptor)
{
}
