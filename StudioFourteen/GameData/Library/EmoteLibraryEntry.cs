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

namespace StudioFourteen.GameData.Library;

using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Library.Sources;
using StudioFourteen.Services;

public class EmoteLibraryEntry(SourceBase source, Emote emote)
	: ExcelLibraryEntry(source, emote.RowId)
{
	public override string? Name => emote.Name.GetString();
	public override object? Icon => new ImageReference(emote.Icon);

	public override bool IsValid
	{
		get
		{
			if (!base.IsValid)
				return false;

			foreach(var rowRef in emote.ActionTimeline)
			{
				if (rowRef.RowId != 0)
				{
					return true;
				}
			}

			return false;
		}
	}

	[LibraryMenu("Execute")]
	public async Task ExecuteEmote()
	{
		await TickService.GameTick();
		unsafe
		{
			if (!EmoteManager.Instance()->CanExecuteEmote((ushort)emote.RowId))
				return;

			EmoteManager.Instance()->ExecuteEmote((ushort)emote.RowId);
		}
	}
}