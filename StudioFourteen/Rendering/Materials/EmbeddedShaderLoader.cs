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
using System.Reflection;
using System.Text;
using SharpDX;
using SharpDX.D3DCompiler;

public abstract class ShaderLoader
{
	public ShaderBytecode? Bytecode { get; private set; }
	public Result ResultCode { get; private set; }
	public bool HasErrors => this.ResultCode.Failure;
	public string? Message { get; private set; }

	public void Load()
	{
		CompilationResult result = this.LoadShader();
		this.Bytecode = result.Bytecode;
		this.ResultCode = result.ResultCode;
		this.Message = result.Message;
	}

	protected abstract CompilationResult LoadShader();
}

public abstract class HlslShaderLoader(string profile, string entryPoint = "Main", ShaderFlags flags = ShaderFlags.None)
	: ShaderLoader
{
	protected sealed override CompilationResult LoadShader()
	{
		string hlsl = this.GetHlsl();

		#if DEBUG
		{
			flags |= ShaderFlags.Debug;
		}
		#endif

		return ShaderBytecode.Compile(hlsl, entryPoint, profile, flags);
	}

	protected abstract string GetHlsl();
}

public class EmbeddedShaderLoader(string file, string profile, string entryPoint = "Main", ShaderFlags flags = ShaderFlags.None)
	: HlslShaderLoader(profile, entryPoint, flags)
{
	protected override string GetHlsl()
	{
		StringBuilder hlslBuilder = new();
		GetShader(file, ref hlslBuilder, 0);
		return hlslBuilder.ToString();
	}

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
			if (line == null)
				continue;

			line = line.Trim();

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