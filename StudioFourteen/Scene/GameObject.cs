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

namespace StudioFourteen.Scene;

using FFXIVClientStructs.FFXIV.Client.Game.Object;
using StudioFourteen.Services.Numerics;
using StudioFourteen.Services.Scene;
using StudioFourteen.Services.Tick;
using System.Numerics;

using XivGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

public partial class GameObject : TransformObjectBase
{
	private Transform? nextTransform;
	private Vector3 lastPosition = Vector3.Zero;
	private Quaternion lastRotation = Quaternion.Identity;
	private Vector3 lastScale = Vector3.Zero;

	public GameObject(int objectIndex)
	{
		this.ObjectIndex = objectIndex;
		this.Name = $"{objectIndex}";
	}

	public int ObjectIndex { get; private set; }
	public bool WasAddedAsDefaultObject { get; set; } = false;

	public override string Id => $"GameObject:{this.ObjectIndex}";

	public override double TranslationChange => 0.1f;

	public Bounds Bounds { get; protected set; } = Bounds.Zero;

	public override bool IsHit(HitInfo hitInfo)
	{
		return hitInfo.ObjectTableIndex == this.ObjectIndex;
	}

	public override void OnHovered(bool value)
	{
		base.OnHovered(value);
	}

	public unsafe override void OnGameTick()
	{
		base.OnGameTick();

		XivGameObject* pGameObject = this.GetXivGameObject();
		if (pGameObject == null || pGameObject->DrawObject == null)
			return;

		/*if (this.IsHovered)
		{
			pGameObject->Highlight(ObjectHighlightColor.Yellow);
		}
		else if (this.IsSelected)
		{
			pGameObject->Highlight(ObjectHighlightColor.Orange);
		}
		else
		{
			pGameObject->Highlight(ObjectHighlightColor.None);
		}*/

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
				Studio.Log.Warning("Failed to decompose transform for game object");
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

			this.nextTransform = null;
		}

		this.IsReady = true;
	}

	public unsafe XivGameObject* GetXivGameObject()
	{
		return Studio.Scene.GetXivObject(this.ObjectIndex);
	}

	public unsafe bool CanDraw()
	{
		TickService.VerifyGameTickThread();

		XivGameObject* pGameObject = this.GetXivGameObject();
		if (!pGameObject->IsReadyToDraw())
			return false;

		return pGameObject->RenderFlags == (int)VisibilityFlags.None;
	}

	protected override void LocalTransformChanged(Transform oldValue, Transform newValue)
	{
		base.LocalTransformChanged(oldValue, newValue);
		this.nextTransform = newValue;
	}

	protected override void WorldTransformChanged(Transform oldValue, Transform newValue)
	{
		base.WorldTransformChanged(oldValue, newValue);
		this.nextTransform = newValue;
	}

	protected override void LockTransformChanged(bool oldValue, bool newValue)
	{
		base.LockTransformChanged(oldValue, newValue);
	}
}
