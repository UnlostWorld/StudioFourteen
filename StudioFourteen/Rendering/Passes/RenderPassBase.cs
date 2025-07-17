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

namespace StudioFourteen.Rendering.Passes;

using System;
using Serilog;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

public abstract class RenderPassBase : IDisposable
{
	public ILogger Log;

	public RenderPassBase()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	public ServiceManager Services => ServiceManager.Instance;
	public bool IncludeInScreenshots { get; set; } = false;

	public abstract void Render(Renderer renderer, Device device, DeviceContext deviceContext);

	public virtual void OnResolutionChanging()
	{
	}

	public virtual void OnResolutionChanged()
	{
	}

	public virtual void Dispose()
	{
	}
}

public abstract class InstanceRenderPassBase<T> : RenderPassBase
	where T : unmanaged
{
	public T PassData;
	private Buffer? constantsBuffer;

	public override void Render(Renderer renderer, Device device, DeviceContext deviceContext)
	{
		if (this.constantsBuffer == null)
		{
			this.constantsBuffer = new(
				device,
				SharpDX.Utilities.SizeOf<T>(),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0);
		}

		deviceContext.VertexShader.SetConstantBuffer(Registers.PerPassData, this.constantsBuffer);
		deviceContext.GeometryShader.SetConstantBuffer(Registers.PerPassData, this.constantsBuffer);
		deviceContext.PixelShader.SetConstantBuffer(Registers.PerPassData, this.constantsBuffer);

		deviceContext.UpdateSubresource(ref this.PassData, this.constantsBuffer);
	}

	public override void Dispose()
	{
		base.Dispose();
		this.constantsBuffer?.Dispose();
		this.constantsBuffer = null;
	}
}