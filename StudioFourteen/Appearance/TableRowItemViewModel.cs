namespace StudioFourteen.Appearance;

using StudioFourteen.GameData;
using StudioFourteen.GameData.Excel;
using StudioFourteen.Mvm;
using StudioFourteen.Utilities;

public abstract class TableRowItemViewModel<T> : GearViewModelBase<T>
	where T : LibraryExcelRow
{
	private T? item;

	public TableRowItemViewModel()
	{
	}

	[AutoNotify]
	public sealed override T? Item
	{
		get
		{
			if (!this.HasValidTarget)
				return null;

			if (this.Value == 0)
				return null;

			if (this.item == null)
				this.item = GameDataService.GetRow<T>(this.Value);

			return this.item;
		}

		set
		{
			if (value == null || !value.IsValid)
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
			this.item = GameDataService.GetRow<T>(value);

			if (!this.HasValidTarget)
				return;

			if (this.item == null || !this.item.IsValid)
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
