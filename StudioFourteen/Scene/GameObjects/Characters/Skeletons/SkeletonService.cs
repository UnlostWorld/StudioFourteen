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

namespace StudioFourteen.Scene.GameObjects.Characters.Skeletons;

using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using StudioFourteen.Interop;
using StudioFourteen.Services;
using System;
using System.Collections.Generic;

using XivSkeleton = FFXIVClientStructs.FFXIV.Client.Graphics.Render.Skeleton;

[Service]
public partial class SkeletonService : ServiceBase
{
	private readonly List<Skeleton> skeletons = new();

	public static string? GetMirrorBoneName(string name)
	{
		if (name.EndsWith("_l"))
		{
			return name.Substring(0, name.Length - 2) + "_r";
		}
		else if (name.EndsWith("_r"))
		{
			return name.Substring(0, name.Length - 2) + "_l";
		}

		return null;
	}

	public unsafe override void Attach()
	{
		base.Attach();

		Hooks.UpdateBonePhysics.Enable(this.UpdateBonePhysicsDetour);
		Hooks.SetPosition.Enable(this.SetPosition);
	}

	public override void Detach()
	{
		base.Detach();

		Hooks.UpdateBonePhysics.Disable();
		Hooks.SetPosition.Disable();
	}

	public void AddSkeleton(Skeleton skeleton)
	{
		lock (this.skeletons)
		{
			this.skeletons.Add(skeleton);
		}
	}

	public void RemoveSkeleton(Skeleton skeleton)
	{
		lock (this.skeletons)
		{
			this.skeletons.Remove(skeleton);
		}
	}

	private unsafe void SetPosition(GameObject* self, float x, float y, float z)
	{
		if (this.Services.GroupPose.IsGroupPoseLoaded)
			return;

		Hooks.SetPosition.Original(self, x, y, z);
	}

	private unsafe nint UpdateBonePhysicsDetour(nint a1)
	{
		nint result = Hooks.UpdateBonePhysics.Original(a1);

		try
		{
			if (this.Services.Studio.IsOpen)
			{
				this.UpdateBonePhysics();
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error during skeleton update");
		}

		return result;
	}

	// This is a very hot path, be careful how much you do here.
	// All the main skeleton stuff like positions, IK and physics is done at this point.
	private unsafe void UpdateBonePhysics()
	{
		HashSet<nint> modifiedSkeletonPointers = new();

		lock (this.skeletons)
		{
			foreach (Skeleton skeleton in this.skeletons)
			{
				skeleton.OnUpdateBonePhysics(ref modifiedSkeletonPointers);
			}

			foreach (Skeleton skeleton in this.skeletons)
			{
				skeleton.OnFinalizeSkeleton();
			}
		}

		// Update sub partials
		foreach (nint skeletonPtr in modifiedSkeletonPointers)
		{
			XivSkeleton* skeleton = (XivSkeleton*)skeletonPtr;
			if (skeleton == null)
				continue;

			ushort partialCount = skeleton->PartialSkeletonCount;
			if (partialCount <= 1)
				continue;

			for (int partialIdx = 1; partialIdx < partialCount; partialIdx++)
			{
				PartialSkeleton* partialSkeleton = &skeleton->PartialSkeletons[partialIdx];

				if (partialSkeleton->ConnectedBoneIndex >= 0 && partialSkeleton->ConnectedParentBoneIndex >= 0)
				{
					PartialSkeleton* parentPartial = &skeleton->PartialSkeletons[0];

					// assume pose 0
					hkaPose* pose = partialSkeleton->GetHavokPose(0);
					hkaPose* parentPose = parentPartial->GetHavokPose(0);

					hkQsTransformf* transform = pose->AccessBoneModelSpace(partialSkeleton->ConnectedBoneIndex, hkaPose.PropagateOrNot.Propagate);
					hkQsTransformf* parentTransform = parentPose->AccessBoneModelSpace(partialSkeleton->ConnectedParentBoneIndex, hkaPose.PropagateOrNot.DontPropagate);

					transform->Translation = parentTransform->Translation;
					transform->Rotation = parentTransform->Rotation;
					transform->Scale = parentTransform->Scale;
				}
			}
		}
	}
}