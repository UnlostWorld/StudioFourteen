namespace StudioFourteen.Appearance;

using StudioFourteen.GameData.Excel;
using StudioFourteen.Mvm;

public class OrnamentViewModel : ExcelRowItemViewModel<Ornament>
{
	public OrnamentViewModel()
	{
	}

	[AutoNotify]
	public override unsafe bool HasValidTarget
	{
		get
		{
			if (!base.HasValidTarget)
				return false;

			return this.Target->OrnamentData.OrnamentObject != null;
		}
	}

	protected unsafe override ushort LiveValue
	{
		get => this.Target->OrnamentData.OrnamentId;
		set
		{
			// Can only set 0 (no ornament) while in group pose, setting to 0 outside
			// of group pose crashes the game.
			if (value == 0 && !this.Services.GroupPose.IsGroupPosing)
				return;

			this.Target->OrnamentData.SetupOrnament((short)value, 0);
		}
	}

	protected override string GetSearchTitle() => Resources.Find("LOC_Ornament", "Fashion Accessories");
}
