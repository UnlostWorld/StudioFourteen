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

namespace StudioFourteen.Posing;

using Dalamud.Plugin.Services;
using DependencyPropertyGenerator;
using FontAwesome.Sharp;
using StudioFourteen.Mvm;
using StudioFourteen.Selection;
using StudioFourteen.Structs.Extensions;
using StudioFourteen.Utilities;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using TerraFX.Interop.Windows;
using WpfUtils.Extensions;

public class EyeSelectionId(int objectTableIndex) : ISelectionId
{
	public int ObjectTableIndex { get; init; } = objectTableIndex;

	public override SelectionBase Create() => new EyeSelection(this.ObjectTableIndex);

	public override int GetHashCode()
	{
		return HashCode.Combine(this.GetType(), this.ObjectTableIndex);
	}
}

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
			if (this.Selection?.EyeBone == null || !this.Selection.EyeBone.IsReady)
				return Vector3.Zero;

			if (this.trackingEuler != null)
				return this.trackingEuler.Value;

			return this.Selection.EyeBone.LocalTransform.Rotation.ToEuler();
		}

		set
		{
			if (this.Selection?.EyeBone == null || !this.Selection.EyeBone.IsReady)
				return;

			this.trackingEuler = value;

			if (this.Selection.EyeBone.LocalTransform.ToTRS(out Vector3 translation, out Quaternion rotation, out Vector3 scale))
			{
				rotation.FromEuler(value);
				this.Selection.EyeBone.LocalTransform = Transform.FromTRS(translation, rotation, scale);
			}
		}
	}

	[AutoNotify]
	public double IrisSize
	{
		get
		{
			if (this.Selection?.IrisBone == null
				|| !this.Selection.IrisBone.IsReady)
				return -1;

			return this.Selection.IrisBone.LocalTransform.Scale.X;
		}
		set
		{
			if (this.Selection?.IrisBone == null
				|| !this.Selection.IrisBone.IsReady)
				return;

			if (this.Selection.IrisBone.LocalTransform.ToTRS(out Vector3 translation, out Quaternion rotation, out Vector3 scale))
			{
				scale.X = (float)value;
				scale.Y = (float)value;
				scale.Z = (float)value;
				this.Selection.IrisBone.LocalTransform = Transform.FromTRS(translation, rotation, scale);
			}
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

public class EyeSelection : SelectionBase
{
	private MirrorModes mirrorMode = MirrorModes.MirrorTCopyRS;

	public EyeSelection(int objectTableIndex)
	{
		this.ObjectTableIndex = objectTableIndex;
		this.Name = "Eye";
	}

	public override IconChar Icon => IconChar.Eye;
	public override string TypeName => Resources.Find("LOC_Selection_Eye", "Blend");

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

	public int ObjectTableIndex { get; init; }

	public BoneSelection? EyeBone { get; private set; }
	public BoneSelection? IrisBone { get; private set; }

	public override bool CanReset => true;

	public override ISelectionId Id => new EyeSelectionId(this.ObjectTableIndex);

	public override void Reset()
	{
		this.EyeBone?.Reset();
		this.IrisBone?.Reset();
	}

	public override void Activate()
	{
		base.Activate();

		this.EyeBone?.Activate();
		this.IrisBone?.Activate();

		this.Init().Run();
	}

	public override void Deactivate()
	{
		base.Deactivate();

		this.EyeBone?.Deactivate();
		this.IrisBone?.Deactivate();
	}

	public override void OnGameTick()
	{
		base.OnGameTick();

		this.EyeBone?.OnGameTick();
		this.IrisBone?.OnGameTick();
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
		else
		{
			this.Log.Warning("No Eye bone found");
		}

		this.IrisBone = this.Services.Pose.FindBone(this.ObjectTableIndex, "j_f_irisprm_r");
		if (this.IrisBone != null)
		{
			this.IrisBone.Activate();
			this.IrisBone.MirrorMode = this.mirrorMode;
		}
		else
		{
			this.Log.Warning("No Iris bone found");
		}
	}
}