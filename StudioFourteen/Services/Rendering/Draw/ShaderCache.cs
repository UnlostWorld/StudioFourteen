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

namespace StudioFourteen.Services.Rendering.Draw;

using System;
using System.Collections.Generic;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using StudioFourteen.Services.Content;

public class ShaderCache : IDisposable
{
	private readonly Dictionary<Type, Shader?> shaders = new();

	public void Dispose()
	{
		foreach ((Type type, Shader? shader) in this.shaders)
		{
			shader?.Dispose();
		}

		this.shaders.Clear();
	}

	public Shader? GetShader<TMaterialData>(Device device)
		where TMaterialData : unmanaged, IMaterial
	{
		Shader? shader = null;
		if (!this.shaders.TryGetValue(typeof(TMaterialData), out shader) || shader == null)
		{
			shader = new Shader<TMaterialData>();
			this.shaders.Add(typeof(TMaterialData), shader);
		}

		shader.Load(device);
		return shader;
	}
}

public abstract class Shader : IDisposable
{
	public bool IsError { get; protected set; }

	public VertexShader? Vertex { get; protected set; }
	public ShaderSignature? VertexSignature { get; protected set; }
	public PixelShader? Pixel { get; protected set; }
	public GeometryShader? Geometry { get; protected set; }

	public void Dispose()
	{
		this.Vertex?.Dispose();
		this.VertexSignature?.Dispose();
		this.Pixel?.Dispose();
		this.Geometry?.Dispose();
	}

	public abstract void Load(Device device);
}

public class Shader<TMaterialData> : Shader
	where TMaterialData : unmanaged, IMaterial
{
	private IContent<ShaderBytecode>? vertexShaderContent;
	private IContent<ShaderBytecode>? pixelShaderContent;
	private IContent<ShaderBytecode>? geometryShaderContent;

	public override void Load(Device device)
	{
		if (this.IsError)
		{
			bool shouldLoad = false;
			if (this.vertexShaderContent != null && !this.vertexShaderContent.IsLoaded)
				shouldLoad = true;

			if (this.pixelShaderContent != null && !this.pixelShaderContent.IsLoaded)
				shouldLoad = true;

			if (this.geometryShaderContent != null && !this.geometryShaderContent.IsLoaded)
				shouldLoad = true;

			if (!shouldLoad)
			{
				return;
			}
		}

		try
		{
			TMaterialData material = default;

			if (this.vertexShaderContent == null)
				this.vertexShaderContent = material.GetVertexShader();

			if (this.pixelShaderContent == null)
				this.pixelShaderContent = material.GetPixelShader();

			if (this.geometryShaderContent == null)
				this.geometryShaderContent = material.GetGeometryShader();

			ShaderBytecode? vertexByteCode = this.vertexShaderContent?.Get();
			ShaderBytecode? pixelByteCode = this.pixelShaderContent?.Get();
			ShaderBytecode? geometryByteCode = this.geometryShaderContent?.Get();

			this.Vertex?.Dispose();
			this.Pixel?.Dispose();
			this.Geometry?.Dispose();

			this.Vertex = new VertexShader(device, vertexByteCode);
			this.Pixel = new PixelShader(device, pixelByteCode);

			if (geometryByteCode != null)
				this.Geometry = new GeometryShader(device, geometryByteCode);

			this.VertexSignature = ShaderSignature.GetInputSignature(vertexByteCode);
			this.IsError = false;
		}
		catch (Exception ex)
		{
			this.IsError = true;
			Studio.Log.Information(ex, "Failed to load shader");
		}
	}
}