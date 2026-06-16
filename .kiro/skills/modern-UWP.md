# Windows 现代化开发 — UWP 
最后更新时间：2026-06-16

## .NET 10 UWP 配置

**目标框架**: `net10.0-windows10.0.26100.0` | 最低: `10.0.17763.0`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows10.0.26100.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <UseUwp>true</UseUwp>
    <Platforms>x86;x64;arm64</Platforms>
    <PublishAot>true</PublishAot>
  </PropertyGroup>
</Project>
```

**类库额外配置**: `<WindowsAppSDKSelfContained>false</WindowsAppSDKSelfContained>`

## Win2D 使用

**安装**: `<PackageReference Include="Win2D.uwp" Version="1.28.3" />`

**命名空间**:
```csharp
using Microsoft.Graphics.Canvas.UI.Xaml;  // CanvasControl
using Windows.UI;                          // Color (注意不是 Microsoft.UI)
using System.Numerics;                     // Vector2, Matrix3x2
```

**XAML**:
```xml
<canvas:CanvasControl Draw="Canvas_Draw" ClearColor="CornflowerBlue"/>
```

**C# 代码**:
```csharp
void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
{
    var ds = args.DrawingSession;
    ds.Clear(_backgroundColor);
    ds.DrawText("Hello", 100, 100, Colors.White);
    ds.DrawRectangle(50, 50, 200, 100, Colors.Red, 3);
    
    // 动画：更新状态后调用
    sender.Invalidate();
}
```

**三种控件**:
- `CanvasControl` - 手动刷新 (静态/按需)
- `CanvasAnimatedControl` - 自动刷新 (游戏循环)
- `CanvasVirtualControl` - 区域渲染 (超大画布)

## UWP 命名空间规则

**WinUI 2 控件** (NuGet): `Microsoft.UI.Xaml.Controls`  
**UWP 基础类型** (内置): `Windows.UI.Xaml.*`

```csharp
// ✅ 正确
using Windows.UI.Xaml.Controls;  // UserControl, Page
using Windows.UI.Xaml.Input;     // PointerRoutedEventArgs
using Windows.UI;                // Color

// ❌ 错误 - UWP 中不存在
using Microsoft.UI.Xaml.Input;
using Microsoft.UI;  // Color 在这里不存在
```

## 主题响应

```csharp
this.ActualThemeChanged += (s, e) => ApplyTheme();

void ApplyTheme()
{
    bool isDark = ActualTheme == ElementTheme.Dark;
    _bgColor = isDark ? Color.FromArgb(255,30,30,30) : Colors.White;
    _canvas?.Invalidate();
}
```

## 性能优化 - 虚拟化渲染

```csharp
void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
{
    // 只绘制可见行
    int first = (int)(_scrollViewer.VerticalOffset / LineHeight);
    int last = first + (int)(_scrollViewer.ViewportHeight / LineHeight) + 1;
    
    for (int i = first; i <= last && i < _lines.Count; i++)
        DrawLine(args.DrawingSession, i);
}
```

## 关键注意事项

1. **必须用 Visual Studio 2026 构建** - dotnet CLI 对 UWP XAML 支持不完整
2. **Win2D 不支持 AnyCPU** - 构建时选择 x64/x86/ARM64
3. **资源清理**: `Unloaded` 事件中 Dispose Canvas 资源
4. **批量更新**: 多次修改后调用一次 `Invalidate()`

## 常见错误

| 错误 | 原因 | 解决 |
|------|------|------|
| `UserControl` 找不到 | 用了 `Microsoft.UI.Xaml.Controls` | 改为 `Windows.UI.Xaml.Controls` |
| `Color` 类型错误 | 用了 `Microsoft.UI` | 改为 `Windows.UI` |
| XAML 未生成代码 | 用 dotnet CLI 构建 | 改用 Visual Studio |
| WIN2D0001 警告 | AnyCPU 平台 | 选择具体平台 x64/x86/ARM64 |
