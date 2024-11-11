namespace StudioFourteen.Test;

using StudioFourteen;
using System.Windows;

public partial class App : Application
{
	private readonly ServiceManager services = new();

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		Task.Run(this.services.Start);
	}
}

