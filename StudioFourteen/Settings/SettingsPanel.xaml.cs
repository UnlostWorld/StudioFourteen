namespace StudioFourteen.Settings;

using StudioFourteen.Mvm;
using StudioFourteen.Panels;

public partial class SettingsPanel : Panel
{
	[AutoNotify] public SettingsService.Configuration Settings => this.Services.Settings.Current;
}
