namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Services;
using ScreenshotStudio.Windows;

public partial class SettingsWindow : Panel
{
	[AutoNotify] public SettingsService.Configuration Settings => this.Services.Settings.Current;
}
