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

using System.Runtime.CompilerServices;
using SharpDX.Direct3D11;

public abstract class InstanceMaterialBase<T> : MaterialBase
	where T : unmanaged
{
	private readonly ConditionalWeakTable<DrawObject, Instance> drawObjectDataStore = new();
	private Buffer? dataBuffer;

	public override void Bind(DrawObject obj, Device device, DeviceContext context)
	{
		base.Bind(obj, device, context);

		if (this.dataBuffer == null)
		{
			this.dataBuffer = new(
				device,
				SharpDX.Utilities.SizeOf<T>(),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0);
		}

		context.VertexShader.SetConstantBuffer(Registers.PerMaterialData, this.dataBuffer);
		context.GeometryShader.SetConstantBuffer(Registers.PerMaterialData, this.dataBuffer);
		context.PixelShader.SetConstantBuffer(Registers.PerMaterialData, this.dataBuffer);

		T data = this.GetInstanceData(obj);

		// TODO: Use a buffer array and an index instead of updating every draw call?
		context.UpdateSubresource(ref data, this.dataBuffer);
	}

	public ref T GetInstanceData(DrawObject obj)
	{
		if (!this.drawObjectDataStore.TryGetValue(obj, out var instance))
		{
			instance = new();
			this.SetDefault(ref instance.Data);
			this.drawObjectDataStore.Add(obj, instance);
		}

		return ref instance.Data;
	}

	protected virtual void SetDefault(ref T instance)
	{
	}

	private class Instance
	{
		public T Data;
	}
}