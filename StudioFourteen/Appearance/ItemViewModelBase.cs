// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Appearance;

using Lumina.Excel.Sheets;
using StudioFourteen.GameData.Library;
using StudioFourteen.Mvm;

public abstract class ItemViewModelBase : GearViewModelBase<ItemLibraryEntry>
{
	protected ItemLibraryEntry? item;
	private StainLibraryEntry? stain0;
	private StainLibraryEntry? stain1;

	[AutoNotify] public abstract byte Stain0Id { get; set; }
	[AutoNotify] public abstract byte Stain1Id { get; set; }

	[AutoNotify]
	public StainLibraryEntry? Stain0
	{
		get
		{
			if (!this.HasValidTarget)
				return null;

			if (this.stain0 == null || this.stain0.RowId != this.Stain0Id)
				this.stain0 = this.Services.GameData.GetLibraryEntry<StainLibraryEntry>(this.Stain0Id);

			return this.stain0;
		}

		set
		{
			this.stain0 = value;

			if (this.stain0 != null)
			{
				this.Stain0Id = (byte)this.stain0.RowId;
			}
		}
	}

	[AutoNotify]
	public StainLibraryEntry? Stain1
	{
		get
		{
			if (!this.HasValidTarget)
				return null;

			if (this.stain1 == null || this.stain1.RowId != this.Stain1Id)
				this.stain1 = this.Services.GameData.GetLibraryEntry<StainLibraryEntry>(this.Stain1Id);

			return this.stain1;
		}

		set
		{
			this.stain1 = value;

			if (this.stain1 != null)
			{
				this.Stain1Id = (byte)this.stain1.RowId;
			}
		}
	}

	public unsafe void BackupCharacter()
	{
		this.Services.CharacterAppearance.Backup(this.Target);
	}
}
