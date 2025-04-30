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

namespace StudioFourteen.Rendering.Scene;

using System;
using System.Collections.Generic;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

public abstract class InstanceRendererBase<T> : RendererBase
	where T : unmanaged
{
	public T Data;
	private readonly Dictionary<Type, Buffer> rendererTypeBuffer = new();

	public override void Draw(Transform transform, Device device, DeviceContext deviceContext)
	{
		if (!this.rendererTypeBuffer.TryGetValue(this.GetType(), out Buffer? dataBuffer) || dataBuffer == null)
		{
			dataBuffer = new(
				device,
				SharpDX.Utilities.SizeOf<T>(),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0);

			this.rendererTypeBuffer[this.GetType()] = dataBuffer;
		}

		deviceContext.VertexShader.SetConstantBuffer(Registers.PerRendererData, dataBuffer);
		deviceContext.GeometryShader.SetConstantBuffer(Registers.PerRendererData, dataBuffer);
		deviceContext.PixelShader.SetConstantBuffer(Registers.PerRendererData, dataBuffer);

		// TODO: Use a buffer array and an index instead of updating every draw call?
		deviceContext.UpdateSubresource(ref this.Data, dataBuffer);
	}
}