// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Game/Actor/ActorAppearanceService.cs

namespace StudioFourteen.Appearance;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FontAwesome.Sharp;
using StudioFourteen;
using StudioFourteen.GameData;
using StudioFourteen.Library;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;
using System.Threading.Tasks;

public class CharacterBackupAppearance
	: LibraryEntryBase, ICharacterAppearance, ILibraryActions
{
	private readonly string? name;

	public unsafe CharacterBackupAppearance(Character* character)
		: base(null)
	{
		this.name = character->GetDisplayName();
		this.DrawData = character->DrawData;
		this.ModelId = character->ModelContainer.ModelCharaId;

		this.Tags.Add("Named");

		CustomizeData customize = this.DrawData.CustomizeData;
		this.Icon = customize.GetIcon();
	}

	public CharacterBackupAppearance(Character character)
		: base(null)
	{
		this.name = character.GetDisplayName();
		this.DrawData = character.DrawData;
		this.ModelId = character.ModelContainer.ModelCharaId;

		this.Tags.Add("Named");

		CustomizeData customize = this.DrawData.CustomizeData;
		this.Icon = customize.GetIcon();
	}

	public DrawDataContainer DrawData { get; private set; }
	public int ModelId { get; private set; }
	public override string Name => this.name ?? string.Empty;
	public override string? SubTitle => null;
	public ImageReference? Icon { get; private set; }

	[LibraryMenu(IconChar.Plus, "LOC_AppearanceCreateCharacter")]
	public Task Spawn()
	{
		return ServiceManager.Instance.CharacterLifecycle.CreateAsync(this);
	}

	[LibraryMenuTarget(IconChar.UserShield, "LOC_AppearanceApplyTo")]
	public Task Apply(int objectTableIndex)
	{
		return this.Apply(objectTableIndex, CharacterExtensions.UpdateSource.Library);
	}

	public async Task Apply(int objectTableIndex, CharacterExtensions.UpdateSource source)
	{
		await Threads.FrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return;

		this.Services.CharacterAppearance.SetModelCharaId(objectTableIndex, this.ModelId, source);
		this.Services.CharacterAppearance.SetEquipment(objectTableIndex, this.DrawData.EquipmentModelIds, source);
		this.Services.CharacterAppearance.SetCustomize(objectTableIndex, this.DrawData.CustomizeData, source);
	}

	protected override string GetInternalId() => this.Name;
}
