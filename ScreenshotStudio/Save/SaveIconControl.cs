namespace ScreenshotStudio.Save;

using DependencyPropertyGenerator;
using ScreenshotStudio.Mvm;
using System.Windows;

[RoutedEvent("Saved", RoutedEventStrategy.Direct)]
public partial class SaveIconControl : View
{
	protected override void OnLoaded()
	{
		this.Services.Save.Saved += this.OnSaveSaved;
		base.OnLoaded();
	}

	protected override void OnUnloaded()
	{
		this.Services.Save.Saved -= this.OnSaveSaved;
		base.OnLoaded();
	}

	private void OnSaveSaved()
	{
		this.Dispatcher.BeginInvoke(this.OnSaved);
	}
}