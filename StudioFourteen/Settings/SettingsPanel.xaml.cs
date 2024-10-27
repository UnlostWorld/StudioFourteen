namespace StudioFourteen.Settings;

using StudioFourteen.Input;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using System.Collections.Generic;

public partial class SettingsPanel : Panel
{
	[AutoNotify] public SettingsService.Configuration Settings => this.Services.Settings.Current;

	public Dictionary<KeyBindEvents, KeyBind> Keys
	{
		get
		{
			Dictionary<KeyBindEvents, KeyBind> value = new(this.Services.Input.DefaultKeys);
			foreach((KeyBindEvents evt, KeyBind key) in this.Settings.CustomKeyBinds)
			{
				if (!value.ContainsKey(evt))
					continue;

				value[evt] = key;
			}

			return value;
		}
	}

	private void OnKeyBindChanged(KeyBindEditor sender, KeyBind bind)
	{
		if (sender.Tag is KeyBindEvents evt)
		{
			this.Settings.CustomKeyBinds[evt] = bind;
		}
	}
}
