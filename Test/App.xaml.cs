namespace StudioFourteen.Test;

using StudioFourteen;
using StudioFourteen.GameData;
using System.Windows;

public partial class App : Application
{
	private readonly ServiceManager services = new();

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

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

