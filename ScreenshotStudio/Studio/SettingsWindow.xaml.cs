namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Services;
using ScreenshotStudio.Windows;

public partial class SettingsWindow : PanelWindow
{
	[AutoNotify] public SettingsService.Configuration Config => this.Services.Settings.Current;
}
