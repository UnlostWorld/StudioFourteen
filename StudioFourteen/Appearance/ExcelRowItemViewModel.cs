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

using StudioFourteen.GameData.Library;
using StudioFourteen.Mvm;
using StudioFourteen.Utilities;

public abstract class ExcelRowItemViewModel<TLibraryType> : GearViewModelBase<TLibraryType>
	where TLibraryType : ExcelLibraryEntry
{
	private TLibraryType? item;

	public ExcelRowItemViewModel()
	{
	}

	[AutoNotify]
	public sealed override TLibraryType? Item
	{
		get
		{
			if (!this.HasValidTarget)
				return null;

			if (this.Value == 0)
				return null;

			if (this.item == null)
				this.item = this.Services.GameData.GetLibraryEntry<TLibraryType>(this.Value);

			return this.item;
		}

		set
		{
			if (value == null)
			{
				this.Value = 0;
			}
			else
			{
				this.Value = (ushort)value.RowId;
			}
		}
	}

	// TODO: the same as the customize options, we should maintain a ui-side value, and push/read
	// from the game only during frameworkupdates. look at MenuViewModel for reference.
	[AutoNotify]
	public ushort Value
	{
		get;
		set;
		/*get
		{
			if (!this.HasValidTarget)
				return 0;

			return this.LiveValue;
		}
		set
		{
			this.item = this.Services.GameData.GetLibraryEntry<TLibraryType>(value);

			if (!this.HasValidTarget)
				return;

			if (this.item == null)
			{
				this.LiveValue = 0;
			}
			else
			{
				Threads.RunOnFrameworkThread(() =>
				{
					this.LiveValue = value;
				});
			}
		}*/
	}

	protected unsafe abstract ushort LiveValue
	{
		get;
		set;
	}
}
