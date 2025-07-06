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

namespace StudioFourteen.Scene.GameObjects.Characters;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Appearance;
using StudioFourteen.Library;
using StudioFourteen.Scene.GameObjects.Characters.Skeletons;
using StudioFourteen.Services;
using WpfUtils.Commands;

using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;
using DrawDataContainer = StudioFourteen.Scene.GameObjects.Characters.DrawData.DrawDataContainer;
using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

public class Character : Skeleton
{
	public Character(int objectIndex)
		: base(objectIndex)
	{
		this.RevertAppearanceCommand = new(this.RevertAppearance);
		this.ImportAppearanceCommand = new(this.ImportAppearance);
		this.ExportAppearanceCommand = new(this.ExportAppearance);
	}

	public DrawDataContainer DrawData { get; init; } = new();
	public override object? Icon => Resources.Find("ICON_Type_Character");
	public override string TypeName => Resources.Find("LOC_Type_Character", "Character");

	public SimpleCommand RevertAppearanceCommand { get; init; }
	public SimpleCommand ImportAppearanceCommand { get; init; }
	public SimpleCommand ExportAppearanceCommand { get; init; }

	public unsafe XivCharacter* GetXivCharacter()
	{
		return (XivCharacter*)this.Services.GameObjects.GetXivGameObject(this.ObjectIndex);
	}

	public unsafe CharaMakeType? GetCharaMakeType()
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();
		return pCharacter->DrawData.CustomizeData.GetMakeType();
	}

	public override void OnGameTick()
	{
		base.OnGameTick();
		this.DrawData.OnGameTick(this);
	}

	public async void RevertAppearance()
	{
		await this.Services.CharacterAppearance.Restore(this.ObjectIndex);
	}

	public void ImportAppearance()
	{
		LibraryPanel.Open(this.Services.Panels.GamePanels);
	}

	public async void ExportAppearance()
	{
		AppearanceFile file = new();
		await file.Read(this.ObjectIndex);
		this.Services.Files.SaveFile(file, $"{this.Name}'s Appearance");
	}
}