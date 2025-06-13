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

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FontAwesome.Sharp;
using StudioFourteen.Scene;
using StudioFourteen.Services;
using StudioFourteen.Structs.Extensions;
using StudioFourteen.Utilities;
using System;
using System.Numerics;

public class ObjectTableSelectionId(int objectTableIndex)
	: ISceneObjectId
{
	public int ObjectTableIndex { get; init; } = objectTableIndex;

	public override SceneObjectBase Create() => new ObjectTableSelection(this.ObjectTableIndex);

	public override int GetHashCode()
	{
		return HashCode.Combine(this.ObjectTableIndex);
	}
}

public class ObjectTableSelection : TransformSceneObjectBase
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

	public override object? Icon => Resources.Find("ICON_Selection_Character");
	public override string TypeName => Resources.Find("LOC_Selection_ObjectTable", "Object Table");
	public override bool CanReset => false;

	public override double TranslationChange => 0.1f;

	public override ISceneObjectId Id => new ObjectTableSelectionId(this.ObjectTableId);

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
				gameObject->DrawObject->Scale = Vector3.Clamp(scale, new Vector3(0.1f, 0.1f, 0.1f), new Vector3(1000, 1000, 1000));

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

	public override bool Equals(SceneObjectBase? other)
	{
		if (other is not ObjectTableSelection otherGameObject)
			return false;

		return this.ObjectTableId == otherGameObject.ObjectTableId;
	}

	protected override void OnLocalTransformChanged(Transform oldValue, Transform newValue)
	{
		base.OnLocalTransformChanged(oldValue, newValue);
		this.nextTransform = newValue;
	}

	protected override void OnWorldTransformChanged(Transform oldValue, Transform newValue)
	{
		base.OnWorldTransformChanged(oldValue, newValue);
		this.nextTransform = newValue;
	}

	protected override void OnLockTransformChanged(bool oldValue, bool newValue)
	{
		base.OnLockTransformChanged(oldValue, newValue);
		this.Services.Pose.SetAllBoneReferencesLocked(this.ObjectTableId, newValue);
	}
}