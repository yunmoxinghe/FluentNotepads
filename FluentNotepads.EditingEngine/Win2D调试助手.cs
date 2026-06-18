using System;
using System.Diagnostics;

namespace FluentNotepads.EditingEngine;

/// <summary>
/// Win2D 调试助手 - 用于捕获和诊断 Win2D 相关异常
/// </summary>
internal static class Win2DDebugHelper
{
    /// <summary>
    /// 安全执行操作，捕获并记录任何异常
    /// </summary>
    public static void SafeExecute(Action action, string operationName)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            LogException(operationName, ex);
            throw; // 重新抛出以便调试器捕获
        }
    }

    /// <summary>
    /// 安全执行操作，捕获异常但不重新抛出
    /// </summary>
    public static bool TrySafeExecute(Action action, string operationName)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception ex)
        {
            LogException(operationName, ex);
            return false;
        }
    }

    /// <summary>
    /// 记录异常详情
    /// </summary>
    private static void LogException(string operationName, Exception ex)
    {
        Debug.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.WriteLine($"❌ Win2D 错误: {operationName}");
        Debug.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.WriteLine($"异常类型: {ex.GetType().FullName}");
        Debug.WriteLine($"错误消息: {ex.Message}");
        Debug.WriteLine($"堆栈跟踪:\n{ex.StackTrace}");
        
        if (ex.InnerException != null)
        {
            Debug.WriteLine($"\n内部异常: {ex.InnerException.GetType().FullName}");
            Debug.WriteLine($"内部消息: {ex.InnerException.Message}");
            Debug.WriteLine($"内部堆栈:\n{ex.InnerException.StackTrace}");
        }
        
        Debug.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
    }

    /// <summary>
    /// 检查 Win2D 环境
    /// </summary>
    public static void CheckEnvironment()
    {
        Debug.WriteLine("🔍 Win2D 环境检查:");
        
        try
        {
            Debug.WriteLine($"  ✓ 操作系统版本: {Environment.OSVersion}");
            Debug.WriteLine($"  ✓ 处理器数量: {Environment.ProcessorCount}");
            Debug.WriteLine($"  ✓ 64位系统: {Environment.Is64BitOperatingSystem}");
            Debug.WriteLine($"  ✓ 64位进程: {Environment.Is64BitProcess}");
            
            // 检查 DirectX 支持（通过尝试创建 Win2D 对象）
            using (var device = Microsoft.Graphics.Canvas.CanvasDevice.GetSharedDevice())
            {
                Debug.WriteLine($"  ✓ Win2D 设备创建成功");
                Debug.WriteLine($"  ✓ 设备: {device}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"  ❌ 环境检查失败: {ex.Message}");
        }
        
        Debug.WriteLine("");
    }
}
