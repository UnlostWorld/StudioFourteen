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
using StudioFourteen.Content;
using StudioFourteen.Rendering.Scene;

public abstract class MaterialBase : IDisposable
{
	private VertexShader? vertexShader;
	private PixelShader? pixelShader;
	private GeometryShader? geometryShader;
	private InputLayout? layout;

	private IContent<ShaderBytecode>? vertexShaderContent;
	private IContent<ShaderBytecode>? pixelShaderContent;
	private IContent<ShaderBytecode>? geometryShaderContent;

	public bool IsLoaded
	{
		get
		{
			if (this.vertexShader == null)
				return false;

			if (this.vertexShaderContent?.IsLoaded == false)
				return false;

			if (this.pixelShaderContent?.IsLoaded == false)
				return false;

			if (this.geometryShaderContent?.IsLoaded == false)
				return false;

			return true;
		}
	}

	protected abstract IContent<ShaderBytecode> VertexShader { get; }
	protected abstract IContent<ShaderBytecode> PixelShader { get; }
	protected virtual IContent<ShaderBytecode>? GeometryShader { get; }

	public virtual void Load(Device device)
	{
		if (this.vertexShaderContent == null)
			this.vertexShaderContent = this.VertexShader;

		if (this.pixelShaderContent == null)
			this.pixelShaderContent = this.PixelShader;

		if (this.geometryShaderContent == null)
			this.geometryShaderContent = this.GeometryShader;

		this.vertexShader?.Dispose();
		this.pixelShader?.Dispose();
		this.geometryShader?.Dispose();
		this.layout?.Dispose();

		ShaderBytecode vertexByteCode = this.vertexShaderContent.Get();
		ShaderBytecode pixelByteCode = this.pixelShaderContent.Get();
		ShaderBytecode? geometryByteCode = this.geometryShaderContent?.Get();

		this.vertexShader = new VertexShader(device, vertexByteCode);
		this.pixelShader = new PixelShader(device, pixelByteCode);

		if (geometryByteCode != null)
		{
			this.geometryShader = new GeometryShader(device, geometryByteCode);
		}

		ShaderSignature signature = ShaderSignature.GetInputSignature(vertexByteCode);
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
