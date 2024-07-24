// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Game/Posing/SkeletonService.cs

namespace ScreenshotStudio.Services;

using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using ScreenshotStudio.Structs;
using System;
using System.Threading.Tasks;

public class PoseService : ServiceBase
{
	private string? selectedBone;

	private Hook<UpdateBonePhysicsDelegate>? updateBonePhysicsHook;
	private Hook<FinalizeSkeletonsDelegate>? finalizeSkeletonsHook;

	public delegate void SelectionChangedDelegate(object? newSelection);
	private delegate nint UpdateBonePhysicsDelegate(nint a1);
	private delegate void FinalizeSkeletonsDelegate(nint a1);

	public event SelectionChangedDelegate? SelectionChanged;

	public string? SelectedBone
	{
		get => this.selectedBone;
		set
		{
			this.selectedBone = value;
			this.SelectionChanged?.Invoke(value);
			this.RaisePropertyChanged();
		}
	}

	public override Task Start()
	{
		this.Attach();
		return base.Start();
	}

	public override Task Shutdown()
	{
		this.Detach();
		return base.Shutdown();
	}

	private void Attach()
	{
		this.updateBonePhysicsHook = InteropService.HookFromSignature<UpdateBonePhysicsDelegate>("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC ?? 48 8B 79 ?? 45 33 FF", this.UpdateBonePhysicsDetour);
		this.updateBonePhysicsHook?.Enable();

		// JMP in Framework.TaskRenderGraphicsRender
		this.finalizeSkeletonsHook = InteropService.HookFromSignature<FinalizeSkeletonsDelegate>("40 53 55 57 48 83 EC ?? 65 48 8B 04 25", this.FinalizeSkeletonsHook);
		this.finalizeSkeletonsHook?.Enable();
	}

	private void Detach()
	{
		this.updateBonePhysicsHook?.Dispose();
		this.finalizeSkeletonsHook?.Dispose();
	}

	private nint UpdateBonePhysicsDetour(nint a1)
	{
		if (this.updateBonePhysicsHook == null)
			return 0;

		var result = this.updateBonePhysicsHook.Original(a1);

		try
		{
			this.BeginSkeletonUpdate();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error during skeleton update");
		}

		return result;
	}

	private void FinalizeSkeletonsHook(nint a1)
	{
		if (this.finalizeSkeletonsHook == null)
			return;

		this.finalizeSkeletonsHook.Original(a1);

		try
		{
			this.FinalizeSkeletonUpdate();
		}
		catch (Exception e)
		{
			this.Log.Error(e, "Error during skeleton finalization");
		}
	}

	private void BeginSkeletonUpdate()
	{
		// This is a very hot path, be careful how much you do here.
		// All the main skeleton stuff like positions, IK and physics is done at this point.
		if (!this.Services.GroupPose.IsGroupPosing)
			return;
	}

	private void FinalizeSkeletonUpdate()
	{
		if (!this.Services.GroupPose.IsGroupPosing)
			return;
	}
}

public unsafe class SkeletonViewModel
{
	public SkeletonViewModel(Character* actor)
	{
		CharacterBase* characterBase = (CharacterBase*)actor->GameObject.DrawObject;
	}
}