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

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FontAwesome.Sharp;
using StudioFourteen.History;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

public class GameObjectSelection : TransformSelectionBase
{
	private readonly int objectTableId;
	private string? name;
	private bool isReady = false;

	private Transform lastTransform = default;
	private Transform? nextTransform;

	public GameObjectSelection(int objectTableId)
	{
		this.objectTableId = objectTableId;
	}

	public override bool IsReady => this.isReady;

	public override string Name => this.name ?? "Unknown";
	public override IconChar Icon => IconChar.User;
	public override string? Subtitle => null;
	public override bool CanReset => true;

	public override bool LockTransform
	{
		get => this.Services.Pose.AreAllBoneReferencesLocked(this.objectTableId);
		set => this.Services.Pose.SetAllBoneReferencesLocked(this.objectTableId, value);
	}

	public override Transform WorldTransform
	{
		get => this.LocalTransform;
		set => this.LocalTransform = value;
	}

	public override Transform LocalTransform
	{
		get => this.lastTransform;
		set => this.nextTransform = value;
	}

	public unsafe override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		if (DalamudServices.ObjectTable == null)
			throw new Exception("No Object Table");

		Character* gameObject = (Character*)DalamudServices.ObjectTable.GetObjectAddress(this.objectTableId);
		if (gameObject == null || gameObject->DrawObject == null)
			return;

		this.name = gameObject->GetDisplayName();

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

		this.lastTransform = Transform.FromTRS(
			gameObject->DrawObject->Position,
			gameObject->DrawObject->Rotation,
			gameObject->DrawObject->Scale);

		this.isReady = true;
	}

	public override bool Equals(SelectionBase? other)
	{
		if (other is not GameObjectSelection otherGameObject)
			return false;

		return this.objectTableId == otherGameObject.objectTableId;
	}

	public override void Reset()
	{
		// hmm...
		throw new NotImplementedException();
	}
}