namespace ScreenshotStudio.Settings;

using ScreenshotStudio.Mvm;
using ScreenshotStudio.Panels;

public partial class SettingsPanel : Panel
{
	[AutoNotify] public SettingsService.Configuration Settings => this.Services.Settings.Current;
}
