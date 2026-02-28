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

namespace StudioFourteen.Services.Avalonia;

using System.Numerics;
using System.Runtime.InteropServices;
using global::Avalonia;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using StudioFourteen.Services.Content;
using StudioFourteen.Services.Numerics;
using StudioFourteen.Services.Rendering;
using StudioFourteen.Services.Rendering.Draw;
using StudioFourteen.Services.Rendering.Materials;
using StudioFourteen.Services.Avalonia.Platform;
using System.Collections.Generic;

public partial class WindowRenderer : MeshRenderer<WindowRenderer.AvaloniaUiMaterial>
{
	public readonly WindowImpl Window;
	private readonly DxgiSurface surface;

	private ShaderResourceView? bufferResourceView;
	private Texture2D? texture;

	private ShaderResourceView? subWindowResourceView;

	public WindowRenderer(WindowImpl window, DxgiSurface surface)
		: base(MeshContent.Quad)
	{
		this.Window = window;
		this.surface = surface;

		this.IsVisible = true;
		this.CullMode = CullMode.None;
		this.WriteDepth = false;
		this.StencilMode = Comparison.Always;

		this.Transform = Transform.FromTRS(
			new Vector3(0, 0, 0),
			Quaternion.Identity,
			Vector3.One);
	}

	public Vector4 SubWindowPosition { get; set; }
	public Texture2D? Subwindow { get; set; }

	public override bool Draw(Renderer renderer, Transform transform, Device device, DeviceContext deviceContext)
	{
		if (this.surface.DxgiRenderTarget?.Texture == null)
			return false;

		if (this.texture != this.surface.DxgiRenderTarget.Texture)
		{
			this.bufferResourceView?.Dispose();
			this.bufferResourceView = null;
		}

		if (this.bufferResourceView == null)
		{
			this.texture = this.surface.DxgiRenderTarget.Texture;
			this.bufferResourceView = new(device, this.texture);
			Studio.Log.Verbose("Create new UI render texture resource view");
		}

		deviceContext.PixelShader.SetShaderResource(3, this.bufferResourceView);

		Size windowSize = this.Window.FrameSize ?? new Size(256, 256);
		Vector3 scale = new Vector3(
			(float)windowSize.Width / renderer.Width,
			(float)windowSize.Height / renderer.Height,
			1);

		PixelPoint windowPosition = this.Window.Position;
		float x = -1 + ((float)(windowPosition.X + (windowSize.Width / 2)) / renderer.Width * 2);
		float y = 1 - ((float)(windowPosition.Y + (windowSize.Height / 2)) / renderer.Height * 2);

		Vector3 position = new(x, y, 0);

		this.Transform = Transform.FromTRS(
			position,
			Quaternion.Identity,
			scale);

		this.Material.WindowSize = new Vector2((float)windowSize.Width, (float)windowSize.Height);
		this.Material.CornerRadius = this.Window.CornerRadius;
		this.Material.Margin = this.Window.Margin;

		if (this.Subwindow == null)
		{
			this.Material.SubWindow = Vector4.Zero;
		}
		else
		{
			this.Material.SubWindow = this.SubWindowPosition;

			if (this.subWindowResourceView == null)
			{
				this.subWindowResourceView = new(device, this.Subwindow);
				Studio.Log.Verbose("Create sub window resource view");
			}

			deviceContext.PixelShader.SetShaderResource(4, this.subWindowResourceView);
		}

		return base.Draw(renderer, transform, device, deviceContext);
	}

	public override void Dispose()
	{
		this.bufferResourceView?.Dispose();
		this.bufferResourceView = null;

		base.Dispose();
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct AvaloniaUiMaterial : IMaterial
	{
		public Vector4 CornerRadius;
		public Vector4 Margin;
		public Vector4 SubWindow;
		public Vector2 WindowSize;
		public Vector2 Unused;

		public IContent<ShaderBytecode>? GetVertexShader()
			=> new ShaderReference("Shaders/AvaloniaUiComp.hlsl", "vs_4_0", "vert");
		public IContent<ShaderBytecode>? GetPixelShader()
			=> new ShaderReference("Shaders/AvaloniaUiComp.hlsl", "ps_4_0", "pixel");
		public IContent<ShaderBytecode>? GetGeometryShader()
			=> null;

		public void Initialize()
		{
		}
	}
}