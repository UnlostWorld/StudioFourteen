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
using Serilog;
using SharpDX.Direct3D11;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Rendering.Passes;

using Device = SharpDX.Direct3D11.Device;

public abstract class Renderer : IDisposable
{
	protected readonly ILogger Log;
	private readonly List<RenderPassBase> allPasses = new();

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
	public uint Width { get; private set; }
	public uint Height { get; private set; }

	public ServiceManager Services => ServiceManager.Instance;

	protected Device? Device => this.device;
	protected DeviceContext? DeviceContext => this.deviceContext;
	protected bool CanRender => this.canRender;

	public void Dispose()
	{
		this.deviceContext?.Dispose();
		this.deviceContext = null;

		foreach (RenderPassBase pass in this.allPasses)
		{
			pass.Dispose();
		}
	}

	public void LogInternalError(string message, Exception ex)
	{
		this.Log.Error(ex, message);
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
	protected abstract uint GetDeviceWidth();
	protected abstract uint GetDeviceHeight();
	protected abstract uint GetPendingDeviceWidth();
	protected abstract uint GetPendingDeviceHeight();

	protected void RenderAllPasses()
	{
		this.RenderPasses(this.allPasses);
	}

	private unsafe bool TrySetUpRender()
	{
		if (this.isError)
			return false;

		try
		{
			this.BackBuffer = this.GetBackBuffer();
			if (this.BackBuffer == null)
				return false;

			if (this.BackBuffer.Description.Format != SharpDX.DXGI.Format.R8G8B8A8_UNorm)
				throw new Exception($"wrong format in back buffer texture {this.BackBuffer.Description.Format}");

			this.device = this.BackBuffer.Device;
			if (this.device == null)
				return false;

			if (this.deviceContext == null)
				this.deviceContext = new(this.device);

			uint deviceWidth = this.GetDeviceWidth();
			uint deviceHeight = this.GetDeviceHeight();
			if (this.Width != deviceWidth || this.Height != deviceHeight)
			{
				this.Width = deviceWidth;
				this.Height = deviceHeight;
				this.resolutionChangeCoolDown = 15;

				foreach (RenderPassBase pass in this.allPasses)
				{
					pass.OnResolutionChanged();
				}

				return false;
			}

			uint newDeviceWidth = this.GetPendingDeviceWidth();
			uint newDeviceHeight = this.GetPendingDeviceHeight();
			if (this.Width != newDeviceWidth || this.Height != newDeviceHeight)
			{
				foreach (RenderPassBase pass in this.allPasses)
				{
					pass.OnResolutionChanging();
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