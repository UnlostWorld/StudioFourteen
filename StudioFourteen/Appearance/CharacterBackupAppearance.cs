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

namespace StudioFourteen.Appearance;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.DragAndDrop;
using StudioFourteen.GameData;
using StudioFourteen.Library;
using StudioFourteen.Services;
using System.Threading.Tasks;

using Character = StudioFourteen.Scene.GameObjects.Characters.Character;
using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

public class CharacterBackupAppearance
	: LibraryEntryBase, ICharacterAppearance
{
	private readonly string? name;
	private readonly ImageReference? icon;

	public unsafe CharacterBackupAppearance(XivCharacter* character)
		: base(null)
	{
		this.name = character->GetDisplayName();
		this.DrawData = character->DrawData;
		this.ModelId = character->ModelContainer.ModelCharaId;

		this.Tags.Add("Named");

		CustomizeData customize = this.DrawData.CustomizeData;
		this.icon = customize.GetIcon();
	}

	public CharacterBackupAppearance(XivCharacter character)
		: base(null)
	{
		this.name = character.GetDisplayName();
		this.DrawData = character.DrawData;
		this.ModelId = character.ModelContainer.ModelCharaId;

		this.Tags.Add("Named");

		CustomizeData customize = this.DrawData.CustomizeData;
		this.icon = customize.GetIcon();
	}

	public DrawDataContainer DrawData { get; private set; }
	public int ModelId { get; private set; }
	public override string Name => this.name ?? string.Empty;
	public override string? SubTitle => null;
	public override object? Icon => this.icon;

	public override IDragSceneInstance CreateSceneInstance() => new CharacterAppearanceDragSceneInstance(this);

	public Task Create()
	{
		return ServiceManager.Instance.CharacterLifecycle.CreateAsync(this, UpdateSource.Interface);
	}

	public async Task Apply(Character character, UpdateSource source)
	{
		await TickService.GameTick();

		character.SetModelCharaId(this.ModelId, source);
		character.SetEquipment(this.DrawData.EquipmentModelIds, source);
		character.SetCustomize(this.DrawData.CustomizeData, source);
	}

	protected override string GetInternalId() => this.Name;
}
