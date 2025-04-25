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
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

public abstract class Material : IDisposable
{
	private VertexShader? vertexShader;
	private PixelShader? pixelShader;
	private GeometryShader? geometryShader;
	private InputLayout? layout;

	public bool IsLoaded => this.vertexShader != null;

	public virtual string VertProfile => "vs_4_0";
	public abstract string VertEntryPoint { get; }
	public virtual string PixelProfile => "ps_4_0";
	public abstract string PixelEntryPoint { get; }
	public virtual string GeometryProfile => "gs_4_0";
	public abstract string GeometryEntryPoint { get; }
	public virtual ShaderFlags Flags => ShaderFlags.Debug;

	public virtual void Load(Device device)
	{
		CompilationResult vertexShaderByteCode = ShaderBytecode.Compile(this.GetVertexShader(), this.VertEntryPoint, this.VertProfile, this.Flags);
		this.vertexShader = new VertexShader(device, vertexShaderByteCode);

		CompilationResult pixelShaderByteCode = ShaderBytecode.Compile(this.GetPixelShader(), this.PixelEntryPoint, this.PixelProfile, this.Flags);
		this.pixelShader = new PixelShader(device, pixelShaderByteCode);

		string? geometryHlsl = this.GetGeometryShader();
		if (geometryHlsl != null)
		{
			CompilationResult geometryShaderByteCode = ShaderBytecode.Compile(geometryHlsl, this.GeometryEntryPoint, this.GeometryProfile, this.Flags);
			this.geometryShader = new GeometryShader(device, geometryShaderByteCode);
		}

		ShaderSignature signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
		this.layout = new InputLayout(device, signature, default(Vertex).GetInputElements());
	}

	public void Bind(DeviceContext context)
	{
		context.InputAssembler.InputLayout = this.layout;

		context.VertexShader.Set(this.vertexShader);
		context.PixelShader.Set(this.pixelShader);
		context.GeometryShader.Set(this.geometryShader);
	}

	public void Dispose()
	{
		this.vertexShader?.Dispose();
		this.pixelShader?.Dispose();
		this.geometryShader?.Dispose();
		this.layout?.Dispose();
	}

	protected abstract string GetVertexShader();
	protected abstract string GetPixelShader();
	protected abstract string? GetGeometryShader();
}
