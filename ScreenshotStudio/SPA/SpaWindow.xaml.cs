namespace ScreenshotStudio.SPA;

using ScreenshotStudio.Windows;
using System.Threading.Tasks;
using WpfUtils.Windows;

public partial class SpaWindow : PanelWindow
{
	private static SpaWindow? instance;

	public static void OpenSpa()
	{
		Task.Run(async () =>
		{
			SpaWindow? spa = await PanelWindow.CreatePanelWindow<SpaWindow>();
			if (spa != null)
			{
				instance = spa;

				spa.Dispatcher.Invoke(() =>
				{
					spa.Show();
				});
			}
		});
	}

	public static void CloseSpa()
	{
		if (instance == null)
			return;

		instance.Dispatcher?.Invoke(instance.Close);
	}
}