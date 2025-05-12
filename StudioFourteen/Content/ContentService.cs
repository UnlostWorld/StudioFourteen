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

namespace StudioFourteen.Content;

using System;
using System.Collections.Generic;
using System.IO;
using StudioFourteen.Plugin;
using StudioFourteen.Posing;
using StudioFourteen.Services;

public class ContentService : ServiceBase
{
	private readonly JsonContentReference<Dictionary<string, SimpleViewLayout>> simplePoseLayoutsContent = new("SimplePoseLayouts.jsonc");
	private readonly JsonContentReference<Dictionary<string, BlendTarget>> expressionBlends = new("ExpressionBlends.jsonc");
	private readonly JsonContentReference<HashSet<string>> genitalBones = new("GenitalBones.jsonc");

	public Dictionary<string, SimpleViewLayout>? SimplePoseLayouts => this.simplePoseLayoutsContent.Get();
	public Dictionary<string, BlendTarget>? ExpressionBlends => this.expressionBlends.Get();
	public HashSet<string>? GenitalBones => this.genitalBones.Get();

	public Stream GetContent(string path)
	{
		FileStream stream = new(this.ResolvePath(path), FileMode.Open, FileAccess.Read);
		if (stream == null)
			throw new Exception($"Content \"{path}\" not found in content directory");

		return stream;
	}

	private string ResolvePath(string path)
	{
		FileInfo? assembly = DalamudServices.PluginInterface?.AssemblyLocation;
		if (assembly == null)
			return path;

		return $"{assembly.DirectoryName}/Content/Base/{path}";
	}
}
