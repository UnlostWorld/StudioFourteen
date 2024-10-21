namespace StudioFourteen.Appearance;
using StudioFourteen.GameData;
using StudioFourteen.GameData.Excel;
using StudioFourteen.Mvm;

public abstract class ItemViewModelBase : GearViewModelBase<Item>
{
	protected Item? item;
	private Stain? stain0;
	private Stain? stain1;

	[AutoNotify] public abstract ushort Set { get; set; }
	[AutoNotify] public abstract ushort Base { get; set; }
	[AutoNotify] public abstract ushort Variant { get; set; }
	[AutoNotify] public abstract byte Stain0Id { get; set; }
	[AutoNotify] public abstract byte Stain1Id { get; set; }

	[AutoNotify]
	public Stain? Stain0
	{
		get
		{
			if (!this.HasValidTarget)
				return null;

			if (this.stain0 == null || this.stain0.RowId != this.Stain0Id)
				this.stain0 = GameDataService.GetRow<Stain>(this.Stain0Id);

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
	public Stain? Stain1
	{
		get
		{
			if (!this.HasValidTarget)
				return null;

			if (this.stain1 == null || this.stain1.RowId != this.Stain1Id)
				this.stain1 = GameDataService.GetRow<Stain>(this.Stain1Id);

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
