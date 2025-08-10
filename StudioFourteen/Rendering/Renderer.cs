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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Serilog;
using SharpDX.Direct3D11;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Rendering.Passes;

using Device = SharpDX.Direct3D11.Device;

public abstract class RendererCamera
{
	public abstract Matrix4x4 GetProjectionMatrix(Renderer renderer);
	public abstract Matrix4x4 GetViewMatrix(Renderer renderer);
	public abstract Vector3 GetCameraPosition(Renderer renderer);
}

// Thanks to Pictomancy for much of the initial DX11 Setup logic.
// https://github.com/sourpuh/ffxiv_pictomancy
public abstract class Renderer : IDisposable
{
	protected readonly ILogger Log;
	private readonly List<RenderPassBase> allPasses = new();
	private readonly ShaderCache shaderCache = new();

	private Device? device;
	private DeviceContext? deviceContext;
	private int resolutionChangeCoolDown = 15;

	private bool canRender = false;
	private bool isError = false;

	public Renderer()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	public Texture2D? BackBuffer { get; private set; }
	public int Width { get; private set; } = 256;
	public int Height { get; private set; } = 256;
	public int NewWidth { get; set; } = 256;
	public int NewHeight { get; set; } = 256;
	public abstract RendererCamera Camera { get; }
	public ShaderCache Shaders => this.shaderCache;

	public ServiceManager Services => ServiceManager.Instance;

	protected Device? Device => this.device;
	protected DeviceContext? DeviceContext => this.deviceContext;
	protected bool CanRender => this.canRender;

	public virtual void Dispose()
	{
		this.shaderCache.Dispose();

		this.deviceContext?.Dispose();
		this.deviceContext = null;

		foreach (RenderPassBase pass in this.allPasses)
		{
			pass.Dispose();
		}
	}

	public void HitTest(Vector2 screenPosition, HitTestResult result)
	{
		foreach (RenderPassBase pass in this.allPasses)
		{
			pass.HitTest(this, screenPosition, result);
		}
	}

	public void LogInternalError(string message, Exception ex)
	{
		this.Log.Error(ex, message);
	}

	public virtual void Render()
	{
		this.SetUpRender();
		this.RenderPasses(this.allPasses);

		this.device?.ImmediateContext.Flush();
	}

	protected void SetUpRender()
	{
		this.canRender = this.TrySetUpRender();
	}

	protected void AddPass(RenderPassBase pass)
	{
		this.allPasses.Add(pass);
	}

	protected void RemovePass(RenderPassBase pass)
	{
		this.allPasses.Remove(pass);
	}

	protected virtual void RenderPass(RenderPassBase pass)
	{
		if (this.device == null || this.deviceContext == null)
			return;

		pass.Render(this, this.device, this.deviceContext);
	}

	protected void RenderPasses(IEnumerable<RenderPassBase> passes)
	{
		if (!this.canRender || this.device == null || this.deviceContext == null)
			return;

		// Perform render passes.
		foreach (RenderPassBase pass in passes.ToArray())
		{
			try
			{
				this.RenderPass(pass);
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, $"Error in rendering pass: {pass}");
				this.isError = true;
				return;
			}
		}
	}

	protected unsafe abstract Texture2D? GetBackBuffer();

	protected virtual unsafe bool TrySetUpRender()
	{
		if (this.isError)
			return false;

		try
		{
			this.BackBuffer = this.GetBackBuffer();
			if (this.BackBuffer == null)
				return false;

			this.device = this.BackBuffer.Device;
			if (this.device == null)
				return false;

			if (this.deviceContext == null)
				this.deviceContext = new(this.device);

			this.NewWidth = int.Clamp(this.NewWidth, 128, 4096);
			this.NewHeight = int.Clamp(this.NewHeight, 128, 4096);

			if (this.Width != this.NewWidth || this.Height != this.NewHeight)
			{
				this.Width = this.NewWidth;
				this.Height = this.NewHeight;

				this.Log.Information($"Resolution changed: {this.Width}x{this.Height}");

				foreach (RenderPassBase pass in this.allPasses)
				{
					pass.OnResolutionChanged();
				}

				this.resolutionChangeCoolDown = 15;
				return false;
			}

			if (this.resolutionChangeCoolDown > 0)
			{
				this.resolutionChangeCoolDown--;
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			Logging.Shared.Error(ex, "Error attempting to set up render device");
			this.isError = true;
		}

		return false;
	}
}