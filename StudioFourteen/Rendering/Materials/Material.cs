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
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using StudioFourteen.Rendering.Geometry;

public class Material(string shader) : IDisposable
{
	private VertexShader? vertexShader;
	private PixelShader? pixelShader;
	private InputLayout? layout;

	public bool IsLoaded => this.vertexShader != null;

	public virtual string VertProfile => "vs_4_0";
	public virtual string VertEntryPoint => "vert";
	public virtual string PixelProfile => "ps_4_0";
	public virtual string PixelEntryPoint => "pixel";
	public virtual ShaderFlags Flags => ShaderFlags.Debug;

	public void Load(Device device)
	{
		StringBuilder hlslBuilder = new();
		GetShader(shader, ref hlslBuilder, 0);
		string hlsl = hlslBuilder.ToString();

		CompilationResult vertexShaderByteCode = ShaderBytecode.Compile(hlsl, this.VertEntryPoint, this.VertProfile, this.Flags);
		this.vertexShader = new VertexShader(device, vertexShaderByteCode);

		CompilationResult pixelShaderByteCode = ShaderBytecode.Compile(hlsl, this.PixelEntryPoint, this.PixelProfile, this.Flags);
		this.pixelShader = new PixelShader(device, pixelShaderByteCode);

		ShaderSignature signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
		this.layout = new InputLayout(device, signature, default(Vertex).GetInputElements());
	}

	public void Bind(DeviceContext context)
	{
		context.InputAssembler.InputLayout = this.layout;

		context.VertexShader.Set(this.vertexShader);
		context.PixelShader.Set(this.pixelShader);
	}

	public void Dispose()
	{
		this.vertexShader?.Dispose();
		this.pixelShader?.Dispose();
		this.layout?.Dispose();
	}

	private static void GetShader(string file, ref StringBuilder builder, int depth)
	{
		if (depth >= 100)
		{
			Logging.Shared.Error($"Shader loader exceeded maximum depth for shader: {file}");
			return;
		}

		Assembly assembly = Assembly.GetExecutingAssembly();
		string resourceName = $"StudioFourteen.Rendering.Materials.{file}";
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
