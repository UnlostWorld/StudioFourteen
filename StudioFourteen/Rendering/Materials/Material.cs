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
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using StudioFourteen.Rendering.Scene;

public abstract class MaterialBase : IDisposable
{
	private VertexShader? vertexShader;
	private PixelShader? pixelShader;
	private GeometryShader? geometryShader;
	private InputLayout? layout;

	public bool IsLoaded => this.vertexShader != null;

	protected abstract ShaderLoader VertexShader { get; }
	protected abstract ShaderLoader PixelShader { get; }
	protected virtual ShaderLoader? GeometryShader { get; }

	public virtual void Load(Device device)
	{
		ShaderLoader vertexShaderLoader = this.VertexShader;
		ShaderLoader pixelShaderLoader = this.PixelShader;
		ShaderLoader? geometryShaderLoader = this.GeometryShader;

		vertexShaderLoader.Load();
		pixelShaderLoader.Load();
		geometryShaderLoader?.Load();

		this.vertexShader = new VertexShader(device, vertexShaderLoader.Bytecode);
		this.pixelShader = new PixelShader(device, pixelShaderLoader.Bytecode);

		if (geometryShaderLoader != null)
		{
			this.geometryShader = new GeometryShader(device, geometryShaderLoader.Bytecode);
		}

		ShaderSignature signature = ShaderSignature.GetInputSignature(vertexShaderLoader.Bytecode);
		this.layout = new InputLayout(device, signature, default(Vertex).GetInputElements());
	}

	public virtual void Bind(RendererBase renderer, Device device, DeviceContext context)
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
}
