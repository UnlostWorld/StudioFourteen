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
using Serilog.Events;
using StudioFourteen.Files;
using StudioFourteen.Library.LibraryMenu;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

public class ScriptFile(FileInfo fileInfo, string hash)
	: FileBase
{
	public FileInfo Info { get; set; } = fileInfo;
	public string Hash { get; set; } = hash;
	public string? Code { get; set; }
	public Assembly? Assembly { get; set; }
	public bool HasErrors { get; set; } = false;
	public List<DiagnosticEntry> Diagnostics { get; init; } = new();
	public List<ScriptOption> Options { get; set; } = new();

	[LibraryMenu(IconChar.Robot, "Run")]
	public Task Run()
	{
		return ServiceManager.Instance.Scripting.RunScriptAsync(this);
	}
}

public class DiagnosticEntry(LogEventLevel level, string message, string? location)
{
	public LogEventLevel Level { get; init; } = level;
	public string Message { get; init; } = message;
	public string? Location { get; init; } = location;
}

public class ScriptOption
{
	public enum Types
	{
		CheckBox,
		Toggle,
		Input,
	}

	public Types Type { get; set; }
	public string? Name { get; set; }
	public string? ToolTip { get; set; }
}

public class ScriptHeader : FileBase
{
	public List<ScriptOption> Options { get; set; } = new();
}