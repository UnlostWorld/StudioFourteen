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

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using StudioFourteen.Gizmos;
using StudioFourteen.Selection;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

public class PoseSkeletonOverlay : SelectionOverlayLayerBase
{
	private readonly Dictionary<(BoneId, BoneId), BonePrimitive> primitives = new();
	private int lastTargetIndex = -1;

	public PoseSkeletonOverlay()
		: base("Skeleton")
	{
	}

	public override void Update(Matrix4x4 view, Matrix4x4 projection, Canvas canvas)
	{
		lock (this)
		{
			base.Update(view, projection, canvas);
		}
	}

	public unsafe override void OnFrameworkUpdate()
	{
		base.OnFrameworkUpdate();

		if (this.lastTargetIndex != this.TargetIndex)
		{
			lock (this)
			{
				this.lastTargetIndex = this.TargetIndex;

				foreach (((BoneId boneId, BoneId parentId), BonePrimitive primitive) in this.primitives)
				{
					this.RemoveChild(primitive);
				}

				this.primitives.Clear();

				Character* character = this.Services.Target.GetCharacter(this.TargetIndex);
				if (character == null)
					return;

				CharacterBase* characterBase = character->GetCharacterBase();
				if (characterBase == null)
					return;

				ushort partialCount = characterBase->Skeleton->PartialSkeletonCount;
				for (int partialIdx = 0; partialIdx < partialCount; partialIdx++)
				{
					PartialSkeleton* partialSkeleton = &characterBase->Skeleton->PartialSkeletons[partialIdx];

					byte poseCount = partialSkeleton->GetMaxPoses();
					for (byte poseIdx = 0; poseIdx < poseCount; poseIdx++)
					{
						hkaPose* pose = partialSkeleton->GetHavokPose(poseIdx);
						if (pose == null)
							continue;

						int boneCount = pose->Skeleton->Bones.Length;
						for (short boneIdx = 0; boneIdx < boneCount; boneIdx++)
						{
							hkaBone bone = pose->Skeleton->Bones[boneIdx];
							string? boneName = bone.Name.String;

							BoneId boneId = new(character->ObjectIndex, partialIdx, poseIdx, boneIdx);

							short parentIndex = pose->Skeleton->ParentIndices[boneIdx];
							if (parentIndex != -1)
							{
								BoneId parentId = new(character->ObjectIndex, partialIdx, poseIdx, parentIndex);

								if (!this.primitives.ContainsKey((boneId, parentId)))
								{
									BonePrimitive bonePrimitive = new(boneId, parentId);
									this.primitives.Add((boneId, parentId), bonePrimitive);
									this.AddChild(bonePrimitive);
								}
							}
						}
					}
				}
			}
		}

		if (this.primitives.Count <= 0)
			return;

		foreach (((BoneId boneId, BoneId parentId), BonePrimitive primitive) in this.primitives)
		{
			primitive.OnFrameworkUpdate();
		}
	}
}

public class BonePrimitive : GizmoBase
{
	public Vector3 From;
	public Vector3 To;
	public Color Foreground = Colors.White;
	public int Thickness = 1;

	private readonly BoneReference bone;
	private readonly BoneReference parent;
	private Line? line;

	public BonePrimitive(BoneId bone, BoneId parent)
	{
		this.bone = ServiceManager.Instance.Pose.GetOrCreateBoneReference(bone);
		this.parent = ServiceManager.Instance.Pose.GetOrCreateBoneReference(parent);
	}

	public void OnFrameworkUpdate()
	{
		if (this.bone == null || this.bone.ModelSpaceTransform == null || this.bone.ModelTransform == null)
			return;

		// dots?
		if (this.parent == null || this.parent.ModelSpaceTransform == null || this.parent.ModelTransform == null)
			return;

		Transform parentTransform = this.parent.ModelSpaceTransform.Value * this.parent.ModelTransform.Value;
		Transform boneTransform = this.bone.ModelSpaceTransform.Value * this.bone.ModelTransform.Value;

		this.From = Vector3.Transform(Vector3.Zero, parentTransform.ToMatrix());
		this.To = Vector3.Transform(Vector3.Zero, boneTransform.ToMatrix());
	}

	public override void Enable(Canvas canvas)
	{
		if (this.line == null)
			this.line = this.AddChild<Line>();

		base.Enable(canvas);
	}

	public override void Update()
	{
		if (this.line == null)
			return;

		this.line.StrokeThickness = this.Thickness;

		Vector3 fromPos = this.LocalToScreen(this.From);
		Vector3 toPos = this.LocalToScreen(this.To);

		this.line.X1 = fromPos.X;
		this.line.Y1 = fromPos.Y;
		this.line.X2 = toPos.X;
		this.line.Y2 = toPos.Y;

		if (this.line.Stroke is not SolidColorBrush scb || scb.Color != this.Foreground)
			this.line.Stroke = new SolidColorBrush(this.Foreground);

		this.SetZIndex(this.line, toPos.Z);
	}
}