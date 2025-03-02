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

namespace StudioFourteen.Scripting;

using FontAwesome.Sharp;
using StudioFourteen.Files;
using StudioFourteen.Library.LibraryMenu;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

public class ScriptFile(FileInfo info, string hash, string name, string text)
	: FileBase
{
	public FileInfo Info { get; init; } = info;
	public string Hash { get; init; } = hash;
	public string Text { get; init; } = text;
	public string Name { get; init; } = name;

	public Assembly? Assembly { get; set; }

	[LibraryMenu(IconChar.Robot, "Run")]
	public Task Run()
	{
		return ServiceManager.Instance.Scripting.RunScriptAsync(this);
	}
}