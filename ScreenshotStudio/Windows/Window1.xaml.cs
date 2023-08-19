namespace ScreenshotStudio.Windows;

using System.Threading.Tasks;

public partial class Window1 : PanelWindow
{
	public static new Window1? Show() => PanelWindow.Show<Window1>();
	public static Task<Window1?> ShowAsync() => PanelWindow.ShowAsync<Window1>();
}
