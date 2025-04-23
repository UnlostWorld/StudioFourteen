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
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

public abstract class MaterialBase : IDisposable
{
	private VertexShader? vertexShader;
	private PixelShader? pixelShader;
	private InputLayout? layout;

	public bool IsLoaded => this.vertexShader != null;

	public abstract string Shader { get; }

	public virtual string VertProfile => "vs_4_0";
	public virtual string VertEntryPoint => "vert";
	public virtual string PixelProfile => "ps_4_0";
	public virtual string PixelEntryPoint => "pixel";
	public virtual ShaderFlags Flags => ShaderFlags.Debug;

	public void Load(Device device)
	{
		string name = this.Shader;
		Assembly assembly = Assembly.GetExecutingAssembly();
		string resourceName = $"StudioFourteen.Rendering.Shaders.{name}";
		Stream? stream = assembly.GetManifestResourceStream(resourceName);
		if (stream == null)
			throw new Exception($"Shader \"{name}\" not found in manifest resources");

		using StreamReader reader = new StreamReader(stream);
		string hlsl = reader.ReadToEnd();

		var vertexShaderByteCode = ShaderBytecode.Compile(hlsl, this.VertEntryPoint, this.VertProfile, this.Flags);
		this.vertexShader = new VertexShader(device, vertexShaderByteCode);

		var pixelShaderByteCode = ShaderBytecode.Compile(hlsl, this.PixelEntryPoint, this.PixelProfile, this.Flags);
		this.pixelShader = new PixelShader(device, pixelShaderByteCode);

		var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

		// Layout from VertexShader input signature
		this.layout = new InputLayout(
			device,
			signature,
			[
				new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0),
	        ]);
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
}
