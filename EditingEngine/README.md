# EditingEngine - Reactor 标准架构

本模块为 FluentNotepads 提供基于 Win2D 的高性能文本编辑器，使用 **Microsoft.UI.Reactor** 标准模式实现。

## 架构概览

### 标准 Reactor 组件

```
Win2DTextEditor.cs             ← WinUI UserControl (底层渲染引擎)
Win2DEditorElement             ← 不可变 Element record
Win2DEditorDescriptor          ← ControlDescriptor (声明式配置)
EditingEngineFactories         ← DSL 工厂方法
Win2DEditorExtensions          ← Fluent 修饰符
```

### 文件说明

| 文件 | 作用 | Reactor 模式 |
|------|------|--------------|
| `Win2D文本编辑器.cs` | 底层 WinUI UserControl，处理 Win2D 渲染和用户输入 | 原生控件 |
| `编辑页面.cs` | Reactor Component，定义 Element record 和注册逻辑 | Component + Element |
| `Win2D编辑器元素处理器.cs` | ControlDescriptor 声明式配置 | V1 Protocol |
| `DSL工厂.cs` | 工厂方法 `Win2DEditor(...)` | Factories 模式 |
| `Win2D编辑器扩展.cs` | Fluent 修饰符如 `.Text(...)` | Extension methods |

---

## 使用方式

### 1. 基本用法

```csharp
using static FluentNotepads.EditingEngine.EditingEngineFactories;

public class MyComponent : Component
{
    public override Element Render()
    {
        return VStack(
            TextBlock("My Editor"),
            Win2DEditor("Hello, World!")
        );
    }
}
```

### 2. 响应式状态

```csharp
public override Element Render()
{
    var (text, setText) = UseState("Initial text");
    
    return VStack(
        Button("Load", () => setText("New content...")),
        Win2DEditor(text)
    );
}
```

### 3. Fluent API

```csharp
Win2DEditor()
    .Text("Sample content\nLine 2\nLine 3")
    .Margin(16)
    .Padding(8)
```

---

## 技术细节

### Element 定义（不可变 record）

```csharp
public record Win2DEditorElement(string InitialText) : Element;
```

- 使用 `record` 关键字确保不可变性
- 继承自 `Element` 基类
- 通过 `with` 表达式创建副本

### ControlDescriptor 配置

```csharp
internal static class Win2DEditorDescriptor
{
    internal static readonly ControlDescriptor<Win2DEditorElement, Win2DTextEditor> Descriptor =
        new ControlDescriptor<Win2DEditorElement, Win2DTextEditor>()
            .OneWay(
                get: static e => e.InitialText,
                set: static (c, v) => c.SetText(v));
}

internal sealed class Win2DEditorDescriptorHandler() 
    : DescriptorHandler<Win2DEditorElement, Win2DTextEditor>(Win2DEditorDescriptor.Descriptor)
{
}
```

**优势：**
- ✅ 声明式，无需手写 Mount/Update
- ✅ 自动 diff — 仅当 `InitialText` 改变时调用 `SetText`
- ✅ 支持控件池化，提升性能
- ✅ 类型安全的静态 lambda

### 注册

```csharp
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
```

工厂方法通过 `_ = Win2DEditorReg.Done;` 触发注册。

---

## 与旧模式对比

### ❌ 旧模式（手工 IElementHandler）

```csharp
public class Handler : IElementHandler<Win2DEditorElement, Win2DTextEditor>
{
    public Win2DTextEditor Mount(MountContext ctx, Win2DEditorElement element)
    {
        var editor = ctx.RentControl<Win2DTextEditor>();
        Update(default, element, element, editor);
        return editor;
    }

    public void Update(UpdateContext ctx, Win2DEditorElement oldEl, 
                       Win2DEditorElement newEl, Win2DTextEditor control)
    {
        if (oldEl.InitialText != newEl.InitialText)
            control.SetText(newEl.InitialText);
    }
}
```

**缺点：**
- 手工维护 Mount/Update 逻辑
- 样板代码多
- 难以扩展

### ✅ 新模式（ControlDescriptor）

```csharp
internal static class Win2DEditorDescriptor
{
    internal static readonly ControlDescriptor<Win2DEditorElement, Win2DTextEditor> Descriptor =
        new ControlDescriptor<Win2DEditorElement, Win2DTextEditor>()
            .OneWay(
                get: static e => e.InitialText,
                set: static (c, v) => c.SetText(v));
}

internal sealed class Win2DEditorDescriptorHandler() 
    : DescriptorHandler<Win2DEditorElement, Win2DTextEditor>(Win2DEditorDescriptor.Descriptor)
{
}
```

**优点：**
- 声明式，简洁
- 自动 diff
- 易于添加新属性

---

## 测试

### 单元测试（建议位置）

如果需要测试，应放在 Reactor 项目的标准测试目录：

```
tests/
  Reactor.AppTests.Host/SelfTest/Fixtures/
    Win2DEditorFixtures.cs
```

### 测试示例

```csharp
internal class Win2DEditorBasic(Harness h) : SelfTestFixtureBase(h)
{
    public override async Task RunAsync()
    {
        var host = H.CreateHost();
        host.Mount(ctx =>
        {
            var (text, setText) = ctx.UseState("Initial");
            return VStack(
                Button("Update", () => setText("Updated")),
                Win2DEditor(text)
            );
        });

        await Harness.Render();
        // 验证编辑器已挂载
        var editor = H.FindDescendant<Win2DTextEditor>();
        H.Check("Editor_Mounted", editor is not null);
        H.Check("Editor_InitialText", editor!.GetText() == "Initial");

        // 更新文本
        H.ClickButton("Update");
        await Harness.Render();
        H.Check("Editor_TextUpdated", editor.GetText() == "Updated");
    }
}
```

---

## 扩展指南

### 添加新属性（例如：字体大小）

#### 1. 扩展 Element

```csharp
public record Win2DEditorElement(
    string InitialText,
    float FontSize = 14f
) : Element;
```

#### 2. 在 Win2DTextEditor 添加方法

```csharp
public void SetFontSize(float size)
{
    _textFormat.FontSize = size;
    _canvas.Invalidate();
}
```

#### 3. 更新 Descriptor

```csharp
internal static class Win2DEditorDescriptor
{
    internal static readonly ControlDescriptor<Win2DEditorElement, Win2DTextEditor> Descriptor =
        new ControlDescriptor<Win2DEditorElement, Win2DTextEditor>()
            .OneWay(e => e.InitialText, (c, v) => c.SetText(v))
            .OneWay(e => e.FontSize, (c, v) => c.SetFontSize(v));
}
```

#### 4. 添加 Fluent 方法

```csharp
public static Win2DEditorElement FontSize(this Win2DEditorElement el, float size)
    => el with { FontSize = size };
```

#### 5. 使用

```csharp
Win2DEditor("Hello").FontSize(18)
```

---

## 参考

- **Reactor 文档**: `docs/guide/extensibility-preview.md`
- **测试指南**: `TESTING.md`
- **Agent 规则**: `AGENTS.md`

---

## 状态

✅ **已完成标准化迁移**
- [x] Element record（不可变）
- [x] ControlDescriptor（声明式）
- [x] 工厂方法
- [x] Fluent 扩展
- [x] 控件池化支持

⚠️ **待补充**
- [ ] Selftest fixtures
- [ ] 事件处理（OnTextChanged 等）
- [ ] 更多编辑器属性（字体、颜色、主题等）
- [ ] 撤销/重做支持
