using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FluentNotepads.EditingEngine
{
    /// <summary>
    /// 编辑页面 - 黑色占位
    /// </summary>
    public sealed class EditingPage : Page
    {
        public EditingPage()
        {
            // 黑色背景
            Background = new SolidColorBrush(Colors.Black);
        }
    }
}
