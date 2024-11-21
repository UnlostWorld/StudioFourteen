// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using WpfUtils.Extensions;
using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;

public class ListMenu : MakeMenuViewModel
{
	private Option? selected;

	public ListMenu(CharaMakeType.CharaMakeMenu makeMenu, CustomizeIndex customizeIndex, ToggleModes toggleMode)
		: base(makeMenu, customizeIndex, toggleMode)
	{
		ExcelSheet<Lobby>? sheet = this.Services.GameData.GetSheet<Lobby>();
		if (sheet == null)
			return;

		for (byte i = 0; i < makeMenu.SubMenuNum; i++)
		{
			Lobby lobby = sheet.GetRow(makeMenu.SubMenuParam[i]);
			string name = lobby.Text.GetString() ?? i.ToString();
			name = name.Replace("Type ", string.Empty);
			this.Options.Add(new Option(name, i));
		}

		this.OnValueChanged(0, this.Value);
	}

	public int Count { get; private set; }

	public Option? Selected
	{
		get => this.selected;
		set
		{
			this.selected = value;
			this.RaisePropertyChanged(nameof(this.Selected));

			if (this.selected == null)
			{
				this.Value = 0;
			}
			else
			{
				this.Value = this.selected.Value;
			}
		}
	}

	public FastObservableCollection<Option> Options { get; init; } = new();

	protected override void OnValueChanged(byte oldValue, byte newValue)
	{
		base.OnValueChanged(oldValue, newValue);

		foreach (Option op in this.Options)
		{
			if (op.Value == newValue)
			{
				this.selected = op;
				this.RaisePropertyChanged(nameof(this.Selected));
				break;
			}
		}
	}

	public record Option(string Label, byte Value);
}
