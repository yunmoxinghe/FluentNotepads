# FluentNotepads (New)

一个新的 .NET 10.0 UWP 项目，用于替代有 XAML 编译问题的旧项目。

## 项目概述

这是一个干净的 .NET 10.0 UWP 项目，没有旧项目中的 XAML 编译问题（.g.i.cs 和 .g.cs 文件重复定义）。

## 项目配置

- **目标框架**: net10.0-windows10.0.26100.0
- **最低平台版本**: 10.0.17763.0
- **平台**: x86, x64, arm64
- **运行时标识符**: win-x86, win-x64, win-arm64
- **默认语言**: zh-CN
- **AOT 发布**: 启用
- **UWP**: 启用
- **MSIX 工具**: 启用

## 项目结构

```
FluentNotepads/
├── FluentNotepads.slnx         # 解决方案文件
├── FluentNotepads/             # 主项目
│   ├── App.xaml                # 应用程序 XAML
│   ├── App.xaml.cs             # 应用程序代码
│   ├── MainPage.xaml           # 主页面 XAML  
│   ├── MainPage.xaml.cs        # 主页面代码
│   ├── Package.appxmanifest    # UWP 清单文件
│   ├── FluentNotepads.csproj   # 项目文件
│   ├── Assets/                 # 应用程序资源
│   └── Properties/             # 项目属性
├── .gitignore                  # Git 忽略文件
└── README.md                   # 项目说明
```

## 为什么创建新项目

旧项目 (`d:\fluentapps\repos\yunmoxinghe\FluentNotepads`) 存在严重的 XAML 编译问题：
- CS0101: 命名空间重复定义
- CS0102: 类型重复定义  
- CS0111: 方法重复定义

这些问题是由于 `.g.i.cs`（设计时文件）和 `.g.cs`（编译时文件）同时被包含到编译中造成的。尝试了多种修复方案均无效，因此决定创建干净的新项目。

## 下一步

1. 将需要的代码从旧项目迁移到新项目
2. 添加 `Notepads.Controls` 等项目引用
3. 配置 CI/CD 流程
4. 添加测试和文档

## 构建说明

```bash
# 恢复 NuGet 包
dotnet restore

# 构建项目
dotnet build

# 发布应用
dotnet publish -c Release -r win-x64
```