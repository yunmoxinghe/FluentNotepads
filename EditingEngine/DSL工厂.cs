namespace FluentNotepads.EditingEngine;

/// <summary>
/// DSL 工厂方法 - 遵循标准 Reactor Factories 模式
/// 提供简洁的 API 用于创建编辑器组件
/// 
/// 使用方式: using static FluentNotepads.EditingEngine.EditingEngineFactories;
/// </summary>
public static class EditingEngineFactories
{
    /// <summary>
    /// 创建 Win2D 文本编辑器
    /// </summary>
    /// <param name="initialText">初始文本内容</param>
    /// <returns>Win2D 编辑器元素</returns>
    public static Win2DEditorElement Win2DEditor(string initialText = "")
    {
        // 触发静态注册
        _ = Win2DEditorReg.Done;
        return new Win2DEditorElement(initialText);
    }
}
