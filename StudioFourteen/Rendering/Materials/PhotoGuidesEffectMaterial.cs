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
using System.Diagnostics;
using System.Runtime.InteropServices;
using SharpDX.D3DCompiler;
using StudioFourteen.Content;
using WpfUtils.Animation;

public class PhotoGuidesEffectMaterial : InstanceMaterialBase<PhotoGuidesEffectMaterial.InstanceData>
{
	private const float LerpTimeMs = 250;
	private readonly Stopwatch lerpTimer = new();
	private readonly EasingFunctionBase easing = new SineEase();
	private float fromLeftRight;
	private float fromTopBottom;
	private float toLeftRight;
	private float toTopBottom;

	public enum GuideModes : uint
	{
		None,
		Thirds,
		Crosshair,
	}

	public GuideModes GuidesMode { get; set; }

	protected ServiceManager Services => ServiceManager.Instance;

	protected override IContent<ShaderBytecode> VertexShader => new ShaderReference("Shaders/Blit_PhotoGuides.hlsl", "vs_4_0", "vert");
	protected override IContent<ShaderBytecode> PixelShader => new ShaderReference("Shaders/Blit_PhotoGuides.hlsl", "ps_4_0", "pixel");

	public override void UpdateInstanceData(ref InstanceData instance)
	{
		base.UpdateInstanceData(ref instance);

		instance.GuidesMode = (uint)this.GuidesMode;

		float p = this.lerpTimer.ElapsedMilliseconds / LerpTimeMs;
		p = Math.Clamp(p, 0.0f, 1.0f);
		p = this.easing.Ease(p, EasingFunctionBase.EasingModes.EaseOut);

		instance.LeftRight = float.Lerp(this.fromLeftRight, this.toLeftRight, p);
		instance.TopBottom = float.Lerp(this.fromTopBottom, this.toTopBottom, p);

		if (this.lerpTimer.ElapsedMilliseconds > LerpTimeMs)
		{
			this.lerpTimer.Stop();
		}
	}

	public void SetAspectRatio(double aspect)
	{
		this.fromLeftRight = this.toLeftRight;
		this.fromTopBottom = this.toTopBottom;

		if (aspect == 0)
		{
			this.toTopBottom = 0;
			this.toLeftRight = 0;
			this.lerpTimer.Restart();
			return;
		}

		double height = this.Services.Rendering.Width;
		double width = this.Services.Rendering.Height;

		double currentAspect = height / width;

		this.toTopBottom = 0;
		this.toLeftRight = (1 - (float)(aspect / currentAspect)) / 2;

		if (this.toLeftRight < 0)
		{
			this.toTopBottom = -(float)(this.toLeftRight / currentAspect);
			this.toLeftRight = 0;
		}

		this.lerpTimer.Restart();
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct InstanceData
	{
		public float LeftRight;
		public float TopBottom;
		public uint GuidesMode;
		public float Unused4;
	}
}
