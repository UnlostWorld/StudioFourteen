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

namespace StudioFourteen.Appearance;

using System.Numerics;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.DragAndDrop;
using StudioFourteen.Services;
using StudioFourteen.Utilities;

public interface ICharacterAppearance : IDraggable
{
	string? Name { get; }

	public Task Apply(int objectTableIndex);
	public Task<int> Spawn();
}

public class CharacterAppearanceDragSceneInstance(ICharacterAppearance appearance) : IDragSceneInstance
{
	private int spawnedObjectId = -1;

	public async Task EnterScene()
	{
		if (this.spawnedObjectId != -1)
			return;

		this.spawnedObjectId = await appearance.Spawn();
	}

	public async Task Drop(HitInfo hit)
	{
		await TickService.GameTick();
		this.UpdatePosition(hit);
	}

	public async Task LeaveScene()
	{
		await ServiceManager.Instance.CharacterLifecycle.DestroyAsync(this.spawnedObjectId);
		this.spawnedObjectId = -1;
	}

	public unsafe void UpdatePosition(HitInfo hit)
	{
		TickService.VerifyGameTickThread();

		if (this.spawnedObjectId == -1)
			return;

		Character* pCharacter = ServiceManager.Instance.GameObjects.Get<Character>(this.spawnedObjectId);
		if (pCharacter == null || pCharacter->DrawObject == null)
			return;

		Vector3 lastPos = pCharacter->DrawObject->Position;
		Vector3 move = hit.Position - lastPos;

		if (move.Length() > 0.05)
		{
			// 0-out the y movement so swe dont rotate people sidways.
			lastPos.Y = hit.Position.Y;

			Matrix4x4 mat = Matrix4x4.CreateLookAt(hit.Position, lastPos, Vector3.UnitY);
			Quaternion rot = Quaternion.CreateFromRotationMatrix(mat);
			rot = Quaternion.Identity / Quaternion.Normalize(rot);

			pCharacter->DrawObject->Rotation = rot;
		}

		pCharacter->DrawObject->Position = hit.Position;
	}
}