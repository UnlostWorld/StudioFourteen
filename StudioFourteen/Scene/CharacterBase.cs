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

namespace StudioFourteen.Scene;

using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Lumina.Excel.Sheets;
using StudioFourteen.Services.Tick;
using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

public partial class CharacterBase : Skeleton
{
	public CharacterBase(int objectIndex)
		: base(objectIndex)
	{
		Studio.Portraits.Generate(objectIndex, this.OnPortraitLoaded);

		this.AddGizmo<CharacterGizmo>();
	}

	[ObservableProperty]
	public partial Bitmap? Portrait { get; private set; }

	public unsafe XivCharacter* GetXivCharacter()
	{
		return (XivCharacter*)Studio.Scene.GetXivObject(this.ObjectIndex);
	}

	public void BackupAppearance()
	{
		// TODO
	}

	// ----------------------------------------------------------------------
	// Model Chara Id
	// ----------------------------------------------------------------------
	public unsafe uint GetModelCharaId()
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();
		return (uint)pCharacter->ModelContainer.ModelCharaId;
	}

	public unsafe void SetModelCharaId(ModelChara modelChara, UpdateSource source)
	{
		this.SetModelCharaId((int)modelChara.RowId, source);
	}

	public unsafe void SetModelCharaId(int modelCharaId, UpdateSource source)
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();

		if (pCharacter->ModelContainer.ModelCharaId == modelCharaId)
			return;

		if (source != UpdateSource.Restore && source != UpdateSource.Preview)
			this.BackupAppearance();

		pCharacter->ModelContainer.ModelCharaId = modelCharaId;

		Studio.Redraw.Redraw(this);
	}

	private void OnPortraitLoaded(string path)
	{
		this.Portrait = new Bitmap(path);
	}
}