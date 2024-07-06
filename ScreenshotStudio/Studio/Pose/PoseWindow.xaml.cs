//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Interop/Hooks/PoseHooks.cs

namespace ScreenshotStudio.Studio;

using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using FFXIVClientStructs.Havok.Animation.Rig;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Structs.Extensions;
using ScreenshotStudio.Studio.Pose;
using ScreenshotStudio.Windows;
using System.Windows;

public partial class PoseWindow : ActorWindow
{
	private readonly Hook<SetBoneModelSpaceFfxivDelegate>? setBoneModelSpaceFfxivHook;
	private readonly Hook<CalculateBoneModelSpaceDelegate>? calculateBoneModelSpaceHook;
	private readonly Hook<SyncModelSpaceDelegate>? syncModelSpaceHook;
	private readonly Hook<LookAtIKDelegate>? lookAtIKHook;
	private readonly Hook<AnimFrozenDelegate>? animFrozenHook;
	private readonly Hook<UpdatePosDelegate>? updatePosHook;
	private readonly Hook<SetSkeletonDelegate>? setSkeletonHook;
	private readonly Hook<BustDelegate>? bustHook;

	private bool posingEnabled = false;
	private BoneCollection? selectedBones;

	public unsafe PoseWindow()
	{
		this.setBoneModelSpaceFfxivHook = DalamudServices.HookFromSignature<SetBoneModelSpaceFfxivDelegate>("48 8B C4 48 89 58 18 55 56 57 41 54 41 55 41 56 41 57 48 81 EC ?? ?? ?? ?? 0F 29 70 B8 0F 29 78 A8 44 0F 29 40 ?? 44 0F 29 48 ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8B B1", this.SetBoneModelSpaceFfxivDetour);
		this.calculateBoneModelSpaceHook = DalamudServices.HookFromSignature<CalculateBoneModelSpaceDelegate>("40 53 48 83 EC 10 4C 8B 49 28", this.CalculateBoneModelSpaceDetour);
		this.syncModelSpaceHook = DalamudServices.HookFromSignature<SyncModelSpaceDelegate>("48 83 EC 18 80 79 38 00", this.SyncModelSpaceDetour);
		this.lookAtIKHook = DalamudServices.HookFromSignature<LookAtIKDelegate>("48 8B C4 48 89 58 08 48 89 70 10 F3 0F 11 58 ??", this.LookAtIKDetour);
		this.animFrozenHook = DalamudServices.HookFromSignature<AnimFrozenDelegate>("E8 ?? ?? ?? ?? 0F B6 F0 84 C0 74 0E", this.AnimFrozenDetour);
		this.updatePosHook = DalamudServices.HookFromSignature<UpdatePosDelegate>("E8 ?? ?? ?? ?? EB 29 48 8B 5F 08", this.UpdatePosDetour);
		this.setSkeletonHook = DalamudServices.HookFromSignature<SetSkeletonDelegate>("E8 ?? ?? ?? ?? 48 C1 E5 08", this.SetSkeletonDetour);
		this.bustHook = DalamudServices.HookFromSignature<BustDelegate>("E8 ?? ?? ?? ?? F6 84 24 ?? ?? ?? ?? ?? 0F 28 74 24 ??", this.BustDetour);
	}

	public delegate void BonesChangedEventHandler(BoneCollection? bones);

	private delegate ulong SetBoneModelSpaceFfxivDelegate(nint partialSkeleton, ushort boneId, nint transform, bool enableSecondary, bool enablePropagate);
	private delegate nint CalculateBoneModelSpaceDelegate(ref hkaPose pose, int boneIdx);
	private unsafe delegate void SyncModelSpaceDelegate(hkaPose* pose);
	private unsafe delegate byte* LookAtIKDelegate(byte* a1, long* a2, long* a3, float a4, long* a5, long* a6);
	private unsafe delegate byte AnimFrozenDelegate(uint* a1, int a2);
	private unsafe delegate void UpdatePosDelegate(Actor* a1);
	private unsafe delegate char SetSkeletonDelegate(Skeleton* a1, ushort a2, nint a3);
	private unsafe delegate nint BustDelegate(ActorModel* a1, Bust* a2);

	public event BonesChangedEventHandler? SelectedBonesChanged;

	public BoneCollection? SelectedBones
	{
		get => this.selectedBones;
		set
		{
			this.selectedBones = value;
			this.NotifyPropertyChanged();
			this.SelectedBonesChanged?.Invoke(value);
		}
	}

	[AutoNotify]
	public bool PosingEnabled
	{
		get => this.posingEnabled;

		set
		{
			this.posingEnabled = value;

			if (value)
			{
				this.calculateBoneModelSpaceHook?.Enable();
				this.setBoneModelSpaceFfxivHook?.Enable();
				this.syncModelSpaceHook?.Enable();
				this.lookAtIKHook?.Enable();
				this.updatePosHook?.Enable();
				this.animFrozenHook?.Enable();
				this.setSkeletonHook?.Enable();
				this.bustHook?.Enable();
			}
			else
			{
				this.calculateBoneModelSpaceHook?.Disable();
				this.setBoneModelSpaceFfxivHook?.Disable();
				this.syncModelSpaceHook?.Disable();
				this.lookAtIKHook?.Disable();
				this.updatePosHook?.Disable();
				this.animFrozenHook?.Disable();
				this.setSkeletonHook?.Disable();
				this.bustHook?.Disable();
			}
		}
	}

	[AutoNotify]
	public bool IsBonesWindowOpen
	{
		get => this.Services.Panels.GetIsOpen<BoneWindow>();
		set
		{
			this.Services.Panels.SetIsOpen<BoneWindow>(value);
			this.SavedIsBonesWindowOpen = value;
		}
	}

	public bool SavedIsBonesWindowOpen
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	[AutoNotify]
	public bool ExpandTranslationSliders
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	[AutoNotify]
	public bool ExpandRotationSliders
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	[AutoNotify]
	public bool ExpandScaleSliders
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	[AutoNotify]
	public bool CanPose
	{
		get
		{
			bool canPose = this.Services.Studio.IsOpenAndInGPose;

			if (!canPose && this.PosingEnabled)
				this.PosingEnabled = false;

			return canPose;
		}
	}

	public unsafe hkQsTransformf Transform
	{
		get
		{
			if (!this.HasValidTarget)
				return default;

			if (this.CanPose && this.selectedBones != null)
			{
				return this.selectedBones.Transform;
			}
			else
			{
				return this.Actor->Model->Transform;
			}
		}

		set
		{
			if (this.CanPose && this.selectedBones != null)
			{
				this.selectedBones.Transform = value;
			}
			else
			{
				this.Actor->Model->Transform = value;
			}
		}
	}

	[AutoNotify]
	public unsafe hkQuaternionf? RootRotation
	{
		get
		{
			if (this.selectedBones != null && this.CanPose)
			{
				return this.Actor->Model->Transform.Rotation;
			}
			else
			{
				return null;
			}
		}
	}

	[AutoNotify]
	public float TranslationX
	{
		get => this.Transform.Translation.X;
		set
		{
			hkQsTransformf transform = this.Transform;
			transform.Translation.X = value;
			this.Transform = transform;
		}
	}

	[AutoNotify]
	public float TranslationY
	{
		get => this.Transform.Translation.Y;
		set
		{
			hkQsTransformf transform = this.Transform;
			transform.Translation.Y = value;
			this.Transform = transform;
		}
	}

	[AutoNotify]
	public float TranslationZ
	{
		get => this.Transform.Translation.Z;
		set
		{
			hkQsTransformf transform = this.Transform;
			transform.Translation.Z = value;
			this.Transform = transform;
		}
	}

	[AutoNotify]
	public hkQuaternionf Rotation
	{
		get => this.Transform.Rotation;
		set
		{
			hkQsTransformf transform = this.Transform;
			transform.Rotation = value;
			this.Transform = transform;
		}
	}

	public hkVector4f EulerRotation
	{
		get => this.Transform.Rotation.ToEuler();
		set
		{
			hkQsTransformf transform = this.Transform;
			transform.Rotation = HkQuaternionExtensions.FromEuler(value);
			this.Transform = transform;
		}
	}

	[AutoNotify]
	public float EulerRotationX
	{
		get => this.EulerRotation.X;
		set
		{
			hkVector4f euler = this.EulerRotation;
			euler.X = value;
			this.EulerRotation = euler;
		}
	}

	[AutoNotify]
	public float EulerRotationY
	{
		get => this.EulerRotation.Y;
		set
		{
			hkVector4f euler = this.EulerRotation;
			euler.Y = value;
			this.EulerRotation = euler;
		}
	}

	[AutoNotify]
	public float EulerRotationZ
	{
		get => this.EulerRotation.Z;
		set
		{
			hkVector4f euler = this.EulerRotation;
			euler.Z = value;
			this.EulerRotation = euler;
		}
	}

	[AutoNotify]
	public float ScaleX
	{
		get => this.Transform.Scale.X;
		set
		{
			hkQsTransformf transform = this.Transform;
			transform.Scale.X = value;
			this.Transform = transform;
		}
	}

	[AutoNotify]
	public float ScaleY
	{
		get => this.Transform.Scale.Y;
		set
		{
			hkQsTransformf transform = this.Transform;
			transform.Scale.Y = value;
			this.Transform = transform;
		}
	}

	[AutoNotify]
	public float ScaleZ
	{
		get => this.Transform.Scale.Z;
		set
		{
			hkQsTransformf transform = this.Transform;
			transform.Scale.Z = value;
			this.Transform = transform;
		}
	}

	protected override void OnOpened()
	{
		base.OnOpened();

		this.IsBonesWindowOpen = this.SavedIsBonesWindowOpen;
	}

	protected override void OnClosed()
	{
		this.PosingEnabled = false;

		this.calculateBoneModelSpaceHook?.Dispose();
		this.setBoneModelSpaceFfxivHook?.Dispose();
		this.syncModelSpaceHook?.Dispose();
		this.lookAtIKHook?.Dispose();
		this.updatePosHook?.Dispose();
		this.animFrozenHook?.Dispose();
		this.setSkeletonHook?.Dispose();
		this.bustHook?.Dispose();

		this.Services.Panels.SetIsOpen<BoneWindow>(false);

		base.OnClosed();
	}

	private ulong SetBoneModelSpaceFfxivDetour(nint partialSkeleton, ushort boneId, nint transform, bool enableSecondary, bool enablePropagate)
	{
		return boneId;
	}

	private unsafe nint CalculateBoneModelSpaceDetour(ref hkaPose pose, int boneIdx)
	{
		// This is expected to return the hkQsTransform at the given index in the pose's ModelSpace transform array.
		return (nint)(pose.ModelPose.Data + boneIdx);
	}

	private unsafe void SyncModelSpaceDetour(hkaPose* pose)
	{
	}

	private unsafe char SetSkeletonDetour(Skeleton* a1, ushort a2, nint a3)
	{
		if (this.setSkeletonHook == null)
			return default;

		return this.setSkeletonHook.Original(a1, a2, a3);
	}

	private unsafe nint BustDetour(ActorModel* a1, Bust* a2)
	{
		if (this.bustHook == null)
			return default;

		var exec = this.bustHook.Original(a1, a2);
		////a1->ScaleBust(true);
		return exec;
	}

	private unsafe byte* LookAtIKDetour(byte* a1, long* a2, long* a3, float a4, long* a5, long* a6)
	{
		return (byte*)nint.Zero;
	}

	private unsafe byte AnimFrozenDetour(uint* a1, int a2)
	{
		return 1;
	}

	private unsafe void UpdatePosDetour(Actor* a1)
	{
	}

	private void OnClearSelectionClicked(object sender, RoutedEventArgs e)
	{
		this.SelectedBones = null;
	}
}