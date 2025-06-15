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

namespace StudioFourteen.Scene.GameObjects;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Scene;
using StudioFourteen.Services;
using StudioFourteen.Structs.Extensions;
using StudioFourteen.Utilities;
using System;
using System.Numerics;

using XivGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

public class GameObject : TransformSceneObjectBase
{
	public readonly int ObjectIndex;

	private Transform? nextTransform;
	private Vector3 lastPosition = Vector3.Zero;
	private Quaternion lastRotation = Quaternion.Identity;
	private Vector3 lastScale = Vector3.Zero;

	public GameObject(int objectIndex)
	{
		this.ObjectIndex = objectIndex;
		this.Name = $"{objectIndex}";

		this.Gizmos.Add(new GameObjectGizmo(this));
	}

	public override string Id => $"GameObject:{this.ObjectIndex}";
	public override object? Icon => Resources.Find("ICON_Selection_Character");
	public override string TypeName => Resources.Find("LOC_Selection_ObjectTable", "Object Table");

	public override double TranslationChange => 0.1f;

	public override bool IsHit(HitInfo hitInfo)
	{
		return hitInfo.ObjectTableIndex == this.ObjectIndex;
	}

	public override void OnHovered(bool value)
	{
		base.OnHovered(value);

		bool highlight = value && !this.IsSelected;
	}

	public unsafe override void OnGameTick()
	{
		base.OnGameTick();

		XivGameObject* pGameObject = this.GetXivGameObject();
		if (pGameObject == null || pGameObject->DrawObject == null)
			return;

		this.Name = pGameObject->NameString;

		if (this.nextTransform != null)
		{
			bool success = this.nextTransform.Value.ToTRS(out Vector3 translation, out Quaternion rotation, out Vector3 scale);

			if (success)
			{
				pGameObject->DrawObject->Position = translation;
				pGameObject->DrawObject->Rotation = rotation;

				// do not allow objects to scale below 0, it will break the game.
				pGameObject->DrawObject->Scale = Vector3.Clamp(scale, new Vector3(0.1f, 0.1f, 0.1f), new Vector3(1000, 1000, 1000));

				this.nextTransform = null;
			}
			else
			{
				this.Log.Warning("Failed to decompose transform for game object");
			}
		}

		Vector3 newPosition = pGameObject->DrawObject->Position;
		Quaternion newRotation = pGameObject->DrawObject->Rotation;
		Vector3 newScale = pGameObject->DrawObject->Scale;

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

		this.IsReady = true;
	}

	public unsafe XivGameObject* GetXivGameObject()
	{
		return this.Services.GameObjects.GetXivGameObject(this.ObjectIndex);
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
	}
}