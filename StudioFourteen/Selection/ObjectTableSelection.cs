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

namespace StudioFourteen.Selection;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FontAwesome.Sharp;
using StudioFourteen.Plugin;
using StudioFourteen.Rendering;
using StudioFourteen.Rendering.Gizmos;
using StudioFourteen.Rendering.Scene;
using StudioFourteen.Services;
using StudioFourteen.Structs.Extensions;
using StudioFourteen.Utilities;
using System;
using System.Numerics;

public class ObjectTableSelectionId(int objectTableIndex)
	: ISelectionId
{
	public int ObjectTableIndex { get; init; } = objectTableIndex;

	public override SelectionBase Create() => new ObjectTableSelection(this.ObjectTableIndex);

	public override int GetHashCode()
	{
		return HashCode.Combine(this.ObjectTableIndex);
	}
}

public class ObjectTableSelection : TransformSelectionBase
{
	public readonly int ObjectTableId;

	private Transform? nextTransform;
	private Vector3 lastPosition = Vector3.Zero;
	private Quaternion lastRotation = Quaternion.Identity;
	private Vector3 lastScale = Vector3.Zero;

	public ObjectTableSelection(int objectTableId)
	{
		this.ObjectTableId = objectTableId;
		this.Name = $"{objectTableId}";
	}

	public override IconChar Icon => IconChar.User;
	public override string TypeName => Resources.Find("LOC_Selection_ObjectTable", "Object Table");
	public override bool CanReset => false;
	public override SelectionGizmoBase? Gizmo => new ObjectTableSelectionGizmo();

	public override double TranslationChange => 0.1f;

	public override ISelectionId Id => new ObjectTableSelectionId(this.ObjectTableId);

	public override bool IsHit(HitInfo hitInfo)
	{
		return hitInfo.ObjectTableIndex == this.ObjectTableId;
	}

	public override void OnHovered(bool value)
	{
		base.OnHovered(value);

		bool highlight = value && !this.IsSelected;

		this.Services.Tick.Dispatch(TickService.Channels.GameTick, () =>
		{
			unsafe
			{
				GameObject* gameObject = this.Services.GameObjects.Get(this.ObjectTableId);
				if (gameObject == null || gameObject->DrawObject == null)
					return;

				gameObject->Highlight(highlight ? ObjectHighlightColor.Magenta : ObjectHighlightColor.None);
			}
		});
	}

	public unsafe override void OnGameTick()
	{
		base.OnGameTick();

		GameObject* gameObject = this.Services.GameObjects.Get(this.ObjectTableId);
		if (gameObject == null || gameObject->DrawObject == null)
			return;

		this.Name = gameObject->GetDisplayName();

		if (this.nextTransform != null)
		{
			bool success = this.nextTransform.Value.ToTRS(out Vector3 translation, out Quaternion rotation, out Vector3 scale);

			if (success)
			{
				gameObject->DrawObject->Position = translation;
				gameObject->DrawObject->Rotation = rotation;

				// do not allow objects to scale below 0, it will break the game.
				gameObject->DrawObject->Scale = Vector3.Max(scale, new Vector3(0.1f, 0.1f, 0.1f));

				this.nextTransform = null;
			}
			else
			{
				this.Log.Warning("Failed to decompose transform for game object");
			}
		}

		Vector3 newPosition = gameObject->DrawObject->Position;
		Quaternion newRotation = gameObject->DrawObject->Rotation;
		Vector3 newScale = gameObject->DrawObject->Scale;

		if (!newPosition.IsApproximately(this.lastPosition, 0.001f)
			|| !newRotation.IsApproximately(this.lastRotation, 0.001f)
			|| !newScale.IsApproximately(this.lastScale, 0.001f))
		{
			this.lastPosition = newPosition;
			this.lastRotation = newRotation;
			this.lastScale = newScale;

			Transform newTransform = Transform.FromTRS(newPosition, newRotation, newScale);
			this.LocalTransform = newTransform;
			this.WorldTransform = newTransform;
		}

		this.LockTransform = this.Services.Pose.AreAllBoneReferencesLocked(this.ObjectTableId);

		this.IsReady = true;
	}

	public override bool Equals(SelectionBase? other)
	{
		if (other is not ObjectTableSelection otherGameObject)
			return false;

		return this.ObjectTableId == otherGameObject.ObjectTableId;
	}

	public override void Reset()
	{
		// hmm...
		throw new NotImplementedException();
	}

	protected override void OnLocalTransformChanged(Transform oldValue, Transform newValue)
	{
		this.nextTransform = newValue;
	}

	protected override void OnWorldTransformChanged(Transform oldValue, Transform newValue)
	{
		this.nextTransform = newValue;
	}

	protected override void OnLockTransformChanged(bool oldValue, bool newValue)
	{
		this.Services.Pose.SetAllBoneReferencesLocked(this.ObjectTableId, newValue);
	}
}

public class ObjectTableSelectionGizmo : SelectionGizmo<ObjectTableSelection>
{
	private readonly MeshRenderer circle = new(Meshes.WireCircle, Material.Line);

	public ObjectTableSelectionGizmo()
	{
		this.Add(this.circle);
	}

	public override string Name => "Character Selection";

	protected unsafe override bool Draw(ObjectTableSelection selection)
	{
		GameObject* gameObject = this.Services.GameObjects.Get(selection.ObjectTableId);
		if (gameObject == null || gameObject->DrawObject == null)
			return false;

		float alpha = 0.0f;
		if (selection.IsSelected)
			alpha += 0.75f;
		if (selection.IsHovered)
			alpha += 0.25f;

		////this.circle.Color = new Color(1, 1, 1, alpha);

		this.Transform = Transform.FromScale(gameObject->HitboxRadius / 2, 1, gameObject->HitboxRadius / 2);
		this.Transform *= Transform.FromTRS(gameObject->DrawObject->Position, gameObject->DrawObject->Rotation, gameObject->DrawObject->Scale);

		return true;
	}
}