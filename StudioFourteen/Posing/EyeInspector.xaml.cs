namespace StudioFourteen.Posing;

using DependencyPropertyGenerator;
using FFXIVClientStructs.FFXIV.Common.Lua;
using StudioFourteen.Mvm;
using StudioFourteen.Structs.Extensions;
using StudioFourteen.Utilities;
using System.Numerics;
using System.Threading.Tasks;
using WpfUtils.Extensions;

[DependencyProperty<EyeSelection>("Selection")]
public partial class EyeInspector : View
{
}

public class EyeSelection(int objectTableIndex)
	: SelectionBase
{
	private BoneSelection? eyeBone;
	private MirrorModes mirrorMode = MirrorModes.MirrorTCopyRS;

	public override string Name => "Eye";
	public override string? Subtitle => null;

	public override bool CanMirror => true;
	public override MirrorModes MirrorMode
	{
		get => this.mirrorMode;
		set
		{
			this.mirrorMode = value;

			if (this.eyeBone != null)
			{
				this.eyeBone.MirrorMode = value;
			}
		}
	}

	public int ObjectTableIndex { get; init; } = objectTableIndex;

	[AutoNotify]
	public double LookX
	{
		get => this.EyeEulerRotation.X;
		set
		{
			Vector3 euler = this.EyeEulerRotation;
			euler.X = (float)value;
			this.EyeEulerRotation = euler;
		}
	}

	[AutoNotify]
	public double LookY
	{
		get => -this.EyeEulerRotation.Z;
		set
		{
			Vector3 euler = this.EyeEulerRotation;
			euler.Z = -(float)value;
			this.EyeEulerRotation = euler;
		}
	}

	public Vector3 EyeEulerRotation
	{
		get
		{
			if (this.eyeBone == null)
				return Vector3.Zero;

			return this.eyeBone.LocalRotation.ToEuler();
		}

		set
		{
			if (this.eyeBone == null)
				return;

			Quaternion rotation = this.eyeBone.LocalRotation;
			rotation.FromEuler(value);
			this.eyeBone.LocalRotation = rotation;
		}
	}

	public override bool CanReset => true;

	public override void Reset()
	{
		this.eyeBone?.Reset();
	}

	public override void Activate()
	{
		base.Activate();
		this.Init().Run();
	}

	public override void Deactivate()
	{
		base.Deactivate();
	}

	private async Task Init()
	{
		await Threads.FrameworkThread();

		this.eyeBone = this.Services.Pose.FindBone(this.ObjectTableIndex, "j_f_eye_r");

		if (this.eyeBone != null)
		{
			this.eyeBone.Activate();
			this.eyeBone.MirrorMode = this.mirrorMode;
		}
	}
}