using System;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace FluentNotepads.Pages
{
    /// <summary>
    /// Win2D 测试页面 - 验证 Win2D 在 .NET 10 UWP 中的功能
    /// </summary>
    public sealed partial class Win2DTestPage : Page
    {
        private CanvasBitmap? _bitmap;
        private float _angle = 0f;

        public Win2DTestPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 创建 Win2D 资源（在这里加载图片等）
        /// </summary>
        private void Canvas_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            // 可以在这里异步加载资源
            // 例如: _bitmap = await CanvasBitmap.LoadAsync(sender, "Assets/image.png");
        }

        /// <summary>
        /// Win2D 绘制回调
        /// </summary>
        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var session = args.DrawingSession;
            var width = (float)sender.ActualWidth;
            var height = (float)sender.ActualHeight;

            // 绘制标题文字
            session.DrawText(
                "Win2D 正在运行！",
                new Vector2(width / 2, 50),
                Colors.White,
                new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
                {
                    FontSize = 32,
                    HorizontalAlignment = Microsoft.Graphics.Canvas.Text.CanvasHorizontalAlignment.Center
                });

            // 绘制旋转的矩形
            var centerX = width / 2;
            var centerY = height / 2;

            session.Transform = Matrix3x2.CreateRotation(_angle, new Vector2(centerX, centerY));
            
            session.FillRectangle(
                centerX - 50,
                centerY - 50,
                100,
                100,
                Colors.Yellow);

            session.DrawRectangle(
                centerX - 50,
                centerY - 50,
                100,
                100,
                Colors.Black,
                3);

            session.Transform = Matrix3x2.Identity;

            // 绘制圆形
            session.DrawEllipse(
                centerX + 150,
                centerY,
                60,
                60,
                Colors.Red,
                5);

            session.FillEllipse(
                centerX - 150,
                centerY,
                60,
                60,
                Colors.Green);

            // 绘制线条
            session.DrawLine(
                new Vector2(50, height - 100),
                new Vector2(width - 50, height - 100),
                Colors.White,
                5);

            // 绘制渐变文字
            session.DrawText(
                "GPU 加速的 2D 图形渲染",
                new Vector2(width / 2, height - 50),
                Colors.White,
                new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
                {
                    FontSize = 20,
                    HorizontalAlignment = Microsoft.Graphics.Canvas.Text.CanvasHorizontalAlignment.Center
                });

            // 更新旋转角度（实现动画效果）
            _angle += 0.02f;
            
            // 请求重绘以实现动画
            sender.Invalidate();
        }
    }
}
