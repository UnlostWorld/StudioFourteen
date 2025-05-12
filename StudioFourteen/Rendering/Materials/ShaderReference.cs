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

namespace StudioFourteen.Rendering.Materials;

using System;
using System.IO;
using System.Text;
using SharpDX.D3DCompiler;
using StudioFourteen.Content;

public class ShaderReference(string path, string profile, string entryPoint = "Main", ShaderFlags flags = ShaderFlags.None)
	: ContentReference<ShaderBytecode>(path)
{
	protected override ShaderBytecode Load(Stream stream)
	{
		StringBuilder hlslBuilder = new();
		Load(this.Path, ref hlslBuilder, 0);
		string hlsl = hlslBuilder.ToString();

		#if DEBUG
		{
			flags |= ShaderFlags.Debug;
		}
		#endif

		return ShaderBytecode.Compile(hlsl, entryPoint, profile, flags).Bytecode;
	}

	private static void Load(string file, ref StringBuilder builder, int depth)
	{
		if (depth >= 100)
		{
			Logging.Shared.Error($"Shader loader exceeded maximum depth for shader: {file}");
			return;
		}

		Stream stream = ServiceManager.Instance.Content.GetContent(file);

		if (stream == null)
			throw new Exception($"Shader \"{file}\" not found in manifest resources");

		using StreamReader reader = new StreamReader(stream);
		while(!reader.EndOfStream)
		{
			string? line = reader.ReadLine();
			if (line == null)
				continue;

			line = line.Trim();

			// perform includes
			if (line.StartsWith("#include"))
			{
				string subFile = line.Replace("#include", string.Empty);
				subFile = subFile.Trim();
				subFile = subFile.Trim('\"');

				string? dir = System.IO.Path.GetDirectoryName(file);
				subFile = $"{dir}/{subFile}";

				Load(subFile, ref builder, depth + 1);
			}
			else
			{
				builder.AppendLine(line);
			}
		}
	}
}