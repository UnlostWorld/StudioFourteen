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

namespace StudioFourteen.Utilities;

using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using StudioFourteen.Services;
using System.Numerics;

using CameraManager = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager;

public static class RayCast
{
	public static unsafe HitInfo Cast(Vector2 screenPosition)
	{
		TickService.VerifyGameTickThread();

		HitInfo info = new();

		// Check GameObjects first, characters, etc.
		GameObject* pObject = TargetSystem.Instance()->GetMouseOverObject((int)screenPosition.X, (int)screenPosition.Y);
		if (pObject != null)
		{
			info.GameObject = pObject;
			info.ObjectTableIndex = pObject->ObjectIndex;
			info.Position = pObject->Position;
			return info;
		}

		// Fallback to BG collision to see where the mouse hit the world.
		Camera* pCamera = CameraManager.Instance()->CurrentCamera;
		Ray ray = pCamera->ScreenPointToRay(screenPosition);
		RaycastHit hitInfo;
		bool result = BGCollisionModule.RaycastMaterialFilter(ray.Origin, ray.Direction, out hitInfo);

		if (result)
		{
			info.Position = hitInfo.Point;
		}

		return info;
	}
}

public unsafe class HitInfo
{
	public GameObject* GameObject;
	public int ObjectTableIndex = -1;
	public Vector3 Position;
}