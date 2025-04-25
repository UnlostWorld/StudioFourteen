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

namespace StudioFourteen.Rendering;

using System;
using System.IO;
using System.Reflection;
using System.Text;
using SharpDX.Direct3D11;

public class EmbeddedMaterial(string file, bool hasGeometry = false) : Material
{
	private string? combinedShaderHlsl;

	public override string VertEntryPoint => "vert";
	public override string PixelEntryPoint => "pixel";
	public override string GeometryEntryPoint => "geometry";

	public override void Load(Device device)
	{
		StringBuilder hlslBuilder = new();
		GetShader(file, ref hlslBuilder, 0);
		this.combinedShaderHlsl = hlslBuilder.ToString();

		base.Load(device);

		this.combinedShaderHlsl = null;
	}

	public override string ToString() => $"Material file {file}";

	protected override string GetVertexShader() => this.combinedShaderHlsl ?? string.Empty;
	protected override string GetPixelShader() => this.combinedShaderHlsl ?? string.Empty;
	protected override string? GetGeometryShader() => hasGeometry ? this.combinedShaderHlsl : null;

	private static void GetShader(string file, ref StringBuilder builder, int depth)
	{
		if (depth >= 100)
		{
			Logging.Shared.Error($"Shader loader exceeded maximum depth for shader: {file}");
			return;
		}

		Assembly assembly = Assembly.GetExecutingAssembly();
		string resourceName = $"StudioFourteen.Rendering.Shaders.{file}";
		Stream? stream = assembly.GetManifestResourceStream(resourceName);
		if (stream == null)
			throw new Exception($"Shader \"{file}\" not found in manifest resources");

		using StreamReader reader = new StreamReader(stream);
		while(!reader.EndOfStream)
		{
			string? line = reader.ReadLine();

			// skip empty lines
			if (string.IsNullOrEmpty(line))
				continue;

			line = line.Trim();

			// skip comments
			if (line.StartsWith("//"))
				continue;

			// perform includes
			if (line.StartsWith("#include"))
			{
				string subFile = line.Replace("#include", string.Empty);
				subFile = subFile.Trim();
				subFile = subFile.Trim('\"');

				// TODO: path resolution actually
				subFile = subFile.Replace("../", string.Empty);
				GetShader(subFile, ref builder, depth + 1);
			}
			else
			{
				builder.AppendLine(line);
			}
		}
	}
}