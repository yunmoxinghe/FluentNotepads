# Windows 现代化开发 — UWP 
最后更新时间：2026-06-17

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

## UWP 应用启动（CLI 方式）

### 方式 1: Windows App CLI (推荐) ✨

**安装 winapp CLI**:
```powershell
winget install Microsoft.WinAppCli
# 或
npm install -g @microsoft/winappcli
```

**运行应用** (自动注册 + 启动):
```powershell
# 基本运行
winapp run ".\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64"

# 捕获调试输出和崩溃信息（不能与 --detach 同时使用）
winapp run ".\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64" --debug-output

# 后台运行（不阻塞终端）
winapp run ".\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64" --detach

# 退出时自动卸载（适合测试）
winapp run ".\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64" --unregister-on-exit

# 符号化原生崩溃堆栈（下载 PDB）
winapp run ".\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64" --debug-output --symbols
```

**卸载已注册的包**:
```powershell
winapp unregister <PackageFamilyName>
```

**UI 自动化** (测试/脚本):
```powershell
# 列出所有窗口
winapp ui list-windows -app "YourAppName"

# 检查 UI 树
winapp ui inspect -app "YourAppName" -i

# 点击按钮
winapp ui click "btn-save" -app "YourAppName"

# 截图
winapp ui screenshot -app "YourAppName" -output screenshot.png

# 等待元素出现
winapp ui wait-for "Done" -app "YourAppName" -timeout 10000
```

**优势**:
- ✅ 一行命令自动注册 + 启动
- ✅ 捕获 `OutputDebugString` 日志
- ✅ 自动分析崩溃（minidump + ClrMD/DbgEng）
- ✅ 保留 LocalState 状态
- ✅ 内置 UI Automation（可编程测试）
- ✅ 支持 UWP/WinUI3/WPF/WinForms

### 方式 2: PowerShell 传统方式

**1. 注册开发版本**:
```powershell
Add-AppxPackage -Register "path\to\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\AppxManifest.xml"
```

**2. 查找包信息**:
```powershell
Get-AppxPackage | Where-Object {$_.Name -like "*YourAppName*"} | Select PackageFamilyName
```

**3. 启动应用**:
```powershell
# 方式1: 使用 shell:AppsFolder
Start-Process "shell:AppsFolder\<PackageFamilyName>!App"

# 方式2: 自动查找并启动
$app = Get-AppxPackage | Where-Object {$_.Name -match "YourAppName"}
Start-Process "shell:AppsFolder\$($app.PackageFamilyName)!App"
```

**完整示例**:
```powershell
# 一键注册并启动
Add-AppxPackage -Register ".\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\AppxManifest.xml"
$app = Get-AppxPackage | Where-Object {$_.Name -match "YourAppName"}
Start-Process "shell:AppsFolder\$($app.PackageFamilyName)!App"
```

**注意**: UWP 包应用 exe 文件不能直接双击运行，必须通过包注册后启动。

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

1. **推荐使用 winapp CLI** - 一行命令启动 + 调试输出 + 崩溃分析
2. **必须用 Visual Studio 2026 构建** - dotnet CLI 对 UWP XAML 支持不完整
3. **Win2D 不支持 AnyCPU** - 构建时选择 x64/x86/ARM64
4. **CLI 启动需先注册** - `winapp run` 自动处理，传统方式需手动 `Add-AppxPackage`
5. **资源清理**: `Unloaded` 事件中 Dispose Canvas 资源
6. **批量更新**: 多次修改后调用一次 `Invalidate()`

## 常见错误

| 错误 | 原因 | 解决 |
|------|------|------|
| `UserControl` 找不到 | 用了 `Microsoft.UI.Xaml.Controls` | 改为 `Windows.UI.Xaml.Controls` |
| `Color` 类型错误 | 用了 `Microsoft.UI` | 改为 `Windows.UI` |
| XAML 未生成代码 | 用 dotnet CLI 构建 | 改用 Visual Studio |
| WIN2D0001 警告 | AnyCPU 平台 | 选择具体平台 x64/x86/ARM64 |
| 双击 exe 无反应 | UWP 包应用不能直接运行 | 使用 `winapp run` 或 `shell:AppsFolder` |
| 无法获取日志 | 传统启动方式无输出 | 使用 `winapp run --debug-output` |
