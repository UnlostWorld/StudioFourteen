// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Posing;

using DependencyPropertyGenerator;
using FFXIVClientStructs.FFXIV.Common.Lua;
using StudioFourteen.Mvm;
using StudioFourteen.Structs.Extensions;
using StudioFourteen.Utilities;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfUtils.Extensions;

[DependencyProperty<EyeSelection>("Selection")]
public partial class EyeInspector : View
{
	private Vector3? trackingEuler;

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
			if (this.trackingEuler != null)
				return this.trackingEuler.Value;

			if (this.Selection?.EyeBone == null)
				return Vector3.Zero;

			return this.Selection.EyeBone.LocalTransform.Rotation.ToEuler();
		}

		set
		{
			this.trackingEuler = value;

			if (this.Selection?.EyeBone == null)
				return;

			Transform transform = this.Selection.EyeBone.LocalTransform;
			transform.Rotation.FromEuler(value);
			this.Selection.EyeBone.LocalTransform = transform;
		}
	}

	private void OnLookPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		this.trackingEuler = this.EyeEulerRotation;
	}

	private void OnLookPreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		this.trackingEuler = null;
	}

	partial void OnSelectionChanged()
	{
		this.trackingEuler = null;
	}
}

public class EyeSelection(int objectTableIndex)
	: SelectionBase
{
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

			if (this.EyeBone != null)
			{
				this.EyeBone.MirrorMode = value;
			}
		}
	}

	public int ObjectTableIndex { get; init; } = objectTableIndex;

	public BoneSelection? EyeBone { get; private set; }

	public override bool CanReset => true;

	public override void Reset()
	{
		this.EyeBone?.Reset();
	}

	public override void Activate()
	{
		base.Activate();

		this.EyeBone?.Activate();

		this.Init().Run();
	}

	public override void Deactivate()
	{
		base.Deactivate();
	}

	private async Task Init()
	{
		await Threads.FrameworkThread();

		this.EyeBone = this.Services.Pose.FindBone(this.ObjectTableIndex, "j_f_eye_r");

		if (this.EyeBone != null)
		{
			this.EyeBone.Activate();
			this.EyeBone.MirrorMode = this.mirrorMode;
		}
	}
}