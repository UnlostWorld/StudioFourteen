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
using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

[Inspect("Icons/Character.svg")]
public partial class Character : Skeleton
{
	public Character(int objectIndex)
	: base(objectIndex)
	{
		Studio.Portraits.Generate(objectIndex, this.OnPortraitLoaded);
	}

	[ObservableProperty]
	public partial Bitmap? Portrait { get; private set; }

	[ObservableProperty]
	[Inspect]
	public partial bool Placeholder2 { get; set; }

	public unsafe XivCharacter* GetXivCharacter()
	{
		return (XivCharacter*)Studio.Scene.GetXivObject(this.ObjectIndex);
	}

	private void OnPortraitLoaded(string path)
	{
		this.Portrait = new Bitmap(path);
	}
}