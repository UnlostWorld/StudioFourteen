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
using StudioFourteen.DragAndDrop;
using StudioFourteen.Scene.GameObjects.Characters;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using StudioFourteen.Xaml;

using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

public class CharacterAppearanceDragSceneInstance(ICharacterAppearance appearance) : IDragSceneInstance
{
	private Character? spawnedCharacter = null;

	public object? GetOperationIcon() => XamlResources.Find("ICON_Drag_AddCharacter");

	public async Task EnterScene()
	{
		if (this.spawnedCharacter != null)
			return;

		this.spawnedCharacter = await ServiceManager.Instance.CharacterLifecycle.CreateAsync(appearance, UpdateSource.Preview);
	}

	public async Task Drop(HitInfo hit)
	{
		await TickService.GameTick();
		this.UpdatePosition(hit);
	}

	public async Task LeaveScene()
	{
		if (this.spawnedCharacter != null)
			await ServiceManager.Instance.CharacterLifecycle.DestroyAsync(this.spawnedCharacter.ObjectIndex);

		this.spawnedCharacter = null;
	}

	public unsafe void UpdatePosition(HitInfo hit)
	{
		TickService.VerifyGameTickThread();

		if (this.spawnedCharacter == null)
			return;

		XivCharacter* pCharacter = this.spawnedCharacter.GetXivCharacter();
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
