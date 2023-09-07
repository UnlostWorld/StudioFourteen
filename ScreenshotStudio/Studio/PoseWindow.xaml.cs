// © XivTools.
// Licensed under the MIT license.

//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Interop/Hooks/PoseHooks.cs

namespace ScreenshotStudio.Studio;

using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Structs.Extensions;
using ScreenshotStudio.Studio.Pose;
using ScreenshotStudio.Windows;
using System;

public partial class PoseWindow : ActorWindow
{
	private readonly Hook<SetBoneModelSpaceFfxivDelegate> setBoneModelSpaceFfxivHook;
	private readonly Hook<CalculateBoneModelSpaceDelegate> calculateBoneModelSpaceHook;
	private readonly Hook<SyncModelSpaceDelegate> syncModelSpaceHook;
	private readonly Hook<LookAtIKDelegate> lookAtIKHook;
	private readonly Hook<AnimFrozenDelegate> animFrozenHook;
	private readonly Hook<UpdatePosDelegate> updatePosHook;
	private readonly Hook<SetSkeletonDelegate> setSkeletonHook;
	private readonly Hook<BustDelegate> bustHook;

	private bool posingEnabled = false;

	public unsafe PoseWindow()
	{
		nint setBoneModelSpaceFfxiv = DalamudServices.SigScanner.ScanText("48 8B C4 48 89 58 18 55 56 57 41 54 41 55 41 56 41 57 48 81 EC ?? ?? ?? ?? 0F 29 70 B8 0F 29 78 A8 44 0F 29 40 ?? 44 0F 29 48 ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8B B1");
		this.setBoneModelSpaceFfxivHook = Hook<SetBoneModelSpaceFfxivDelegate>.FromAddress(setBoneModelSpaceFfxiv, this.SetBoneModelSpaceFfxivDetour);

		nint calculateBoneModelSpace = DalamudServices.SigScanner.ScanText("40 53 48 83 EC 10 4C 8B 49 28");
		this.calculateBoneModelSpaceHook = Hook<CalculateBoneModelSpaceDelegate>.FromAddress(calculateBoneModelSpace, this.CalculateBoneModelSpaceDetour);

		nint syncModelSpace = DalamudServices.SigScanner.ScanText("48 83 EC 18 80 79 38 00");
		this.syncModelSpaceHook = Hook<SyncModelSpaceDelegate>.FromAddress(syncModelSpace, this.SyncModelSpaceDetour);

		nint lookAtIK = DalamudServices.SigScanner.ScanText("48 8B C4 48 89 58 08 48 89 70 10 F3 0F 11 58 ??");
		this.lookAtIKHook = Hook<LookAtIKDelegate>.FromAddress(lookAtIK, this.LookAtIKDetour);

		nint animFrozen = DalamudServices.SigScanner.ScanText("E8 ?? ?? ?? ?? 0F B6 F0 84 C0 74 0E");
		this.animFrozenHook = Hook<AnimFrozenDelegate>.FromAddress(animFrozen, this.AnimFrozenDetour);

		nint updatePos = DalamudServices.SigScanner.ScanText("E8 ?? ?? ?? ?? EB 29 48 8B 5F 08");
		this.updatePosHook = Hook<UpdatePosDelegate>.FromAddress(updatePos, this.UpdatePosDetour);

		nint loadSkele = DalamudServices.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 C1 E5 08");
		this.setSkeletonHook = Hook<SetSkeletonDelegate>.FromAddress(loadSkele, this.SetSkeletonDetour);

		nint loadBust = DalamudServices.SigScanner.ScanText("E8 ?? ?? ?? ?? F6 84 24 ?? ?? ?? ?? ?? 0F 28 74 24 ??");
		this.bustHook = Hook<BustDelegate>.FromAddress(loadBust, this.BustDetour);
	}

	private delegate ulong SetBoneModelSpaceFfxivDelegate(nint partialSkeleton, ushort boneId, nint transform, bool enableSecondary, bool enablePropagate);
	private delegate nint CalculateBoneModelSpaceDelegate(ref hkaPose pose, int boneIdx);
	private unsafe delegate void SyncModelSpaceDelegate(hkaPose* pose);
	private unsafe delegate byte* LookAtIKDelegate(byte* a1, long* a2, long* a3, float a4, long* a5, long* a6);
	private unsafe delegate byte AnimFrozenDelegate(uint* a1, int a2);
	private unsafe delegate void UpdatePosDelegate(Actor* a1);
	private unsafe delegate char SetSkeletonDelegate(Skeleton* a1, ushort a2, nint a3);
	private unsafe delegate nint BustDelegate(ActorModel* a1, Bust* a2);

	[AutoNotify]
	public bool PosingEnabled
	{
		get
		{
			if (this.posingEnabled && !this.CanPose)
				this.PosingEnabled = false;

			return this.posingEnabled;
		}

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
				this.setSkeletonHook.Enable();
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
				this.setSkeletonHook.Disable();
				this.bustHook?.Disable();
			}
		}
	}

	[AutoNotify]
	public bool IsBonesWindowOpen
	{
		get => this.Services.Panels.GetIsOpen<BoneWindow>();
		set => this.Services.Panels.SetIsOpen<BoneWindow>(value);
	}

	[AutoNotify]
	public bool CanPose => this.Services.Studio.IsOpenAndInGPose;

	public unsafe ref hkQsTransformf Transform => ref this.Actor->Model->Transform;

	[AutoNotify]
	public float TranslationX
	{
		get => this.Transform.Translation.X;
		set => this.Transform.Translation.X = value;
	}

	[AutoNotify]
	public float TranslationY
	{
		get => this.Transform.Translation.Y;
		set => this.Transform.Translation.Y = value;
	}

	[AutoNotify]
	public float TranslationZ
	{
		get => this.Transform.Translation.Z;
		set => this.Transform.Translation.Z = value;
	}

	public hkVector4f EulerRotation
	{
		get => this.Transform.Rotation.ToEuler();
		set => this.Transform.Rotation = HkQuaternionExtensions.FromEuler(value);
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
		set => this.Transform.Scale.X = value;
	}

	[AutoNotify]
	public float ScaleY
	{
		get => this.Transform.Scale.Y;
		set => this.Transform.Scale.Y = value;
	}

	[AutoNotify]
	public float ScaleZ
	{
		get => this.Transform.Scale.Z;
		set => this.Transform.Scale.Z = value;
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
		this.setSkeletonHook.Dispose();
		this.bustHook?.Dispose();

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
		return this.setSkeletonHook.Original(a1, a2, a3);
	}

	private unsafe nint BustDetour(ActorModel* a1, Bust* a2)
	{
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
}