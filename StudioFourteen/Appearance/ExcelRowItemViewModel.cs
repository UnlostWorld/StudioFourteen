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

	[AutoNotify]
	public ushort Value
	{
		get
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
		}
	}

	protected unsafe abstract ushort LiveValue
	{
		get;
		set;
	}
}
