namespace ScreenshotStudio.Test;

using ScreenshotStudio;
using ScreenshotStudio.GameData;
using ScreenshotStudio.Utilities;
using System.Diagnostics;
using System.Windows;


public partial class App : Application
{
	private readonly ServiceManager services = new();

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		XivWindow.Process = Process.GetProcessesByName("ffxiv_dx11").FirstOrDefault();

		try
		{
			GameDataService.DataProvider = new("C:/Program Files (x86)/Steam/steamapps/common/FINAL FANTASY XIV Online/game/sqpack/");
		}
		catch(Exception)
		{
		}

		Task.Run(this.services.Start);
	}
}

