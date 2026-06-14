---
inclusion: auto
---

# FluentNotepads 项目规则

FluentNotepads 是一个文本编辑应用。同时有winui和winuiforuwp版本。使用reactor进行开发。最佳长期方案
Win2D + DirectWrite + Glyph Cache

## 构建和测试
让用户手动构建和测试。

如果uwp部分编译失败，完善D:\fluentapps\repos\ReactorUWP.core使其兼容microsoft-ui-reactor的语法。优先使用winui2而不是原生uwp。在优化D:\fluentapps\repos\ReactorUWP.core前，先联网获取相关信息。