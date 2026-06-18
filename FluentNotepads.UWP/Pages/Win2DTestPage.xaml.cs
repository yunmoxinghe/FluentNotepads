using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentNotepads.Pages
{
    /// <summary>
    /// Win2D 编辑器测试页面 - 用于诊断 Win2D 渲染问题
    /// </summary>
    public sealed partial class Win2DTestPage : Page
    {
        public Win2DTestPage()
        {
            try
            {
                Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Debug.WriteLine("🧪 Win2DTestPage 构造函数开始");
                Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                
                this.InitializeComponent();
                
                this.Loaded += Win2DTestPage_Loaded;
                
                Debug.WriteLine("✅ Win2DTestPage 构造函数完成");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Win2DTestPage 构造函数异常:");
                Debug.WriteLine($"   {ex.GetType().FullName}: {ex.Message}");
                Debug.WriteLine($"   堆栈: {ex.StackTrace}");
                throw;
            }
        }

        private void Win2DTestPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("📄 Win2DTestPage_Loaded 开始");
                
                UpdateStatus("✅ 页面已加载");
                
                // 设置测试文本
                var testText = @"Welcome to Win2D Editor Test!

This is a test page for diagnosing Win2D rendering issues.

Line 4
Line 5
Line 6

If you can read this text, Win2D is working! 🎉

Try typing below...
";
                
                if (Editor != null)
                {
                    Editor.SetText(testText);
                    UpdateStatus("✅ 编辑器已初始化，文本已设置");
                    Debug.WriteLine("✅ Win2D 编辑器文本已设置");
                }
                else
                {
                    UpdateStatus("❌ 编辑器控件未找到");
                    Debug.WriteLine("❌ Editor 控件为 null");
                }
                
                Debug.WriteLine("✅ Win2DTestPage_Loaded 完成");
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ 错误: {ex.Message}");
                Debug.WriteLine($"❌ Win2DTestPage_Loaded 异常:");
                Debug.WriteLine($"   {ex.GetType().FullName}: {ex.Message}");
                Debug.WriteLine($"   堆栈: {ex.StackTrace}");
            }
        }

        private void Editor_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("📝 Editor_Loaded 事件触发");
                UpdateStatus("✅ Win2D 编辑器已加载到可视树");
                
                // 检查编辑器的尺寸
                if (sender is FrameworkElement editor)
                {
                    Debug.WriteLine($"   编辑器尺寸: {editor.ActualWidth} x {editor.ActualHeight}");
                    UpdateStatus($"✅ 编辑器尺寸: {editor.ActualWidth:F0} x {editor.ActualHeight:F0}");
                    
                    if (editor.ActualWidth == 0 || editor.ActualHeight == 0)
                    {
                        UpdateStatus("⚠️ 警告: 编辑器尺寸为 0，可能无法显示");
                        Debug.WriteLine("⚠️ 警告: 编辑器尺寸为 0");
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Editor_Loaded 错误: {ex.Message}");
                Debug.WriteLine($"❌ Editor_Loaded 异常: {ex.Message}");
            }
        }

        private void UpdateStatus(string message)
        {
            if (StatusText != null)
            {
                StatusText.Text = $"{DateTime.Now:HH:mm:ss.fff} - {message}";
            }
        }
    }
}
