// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Posing;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using StudioFourteen.Plugin;
using System;
using System.Numerics;

public class GameObjectSelection : TransformSelectionBase
{
	private readonly int objectTableId;
	private string? name;
	private bool isReady = false;

	private Transform lastTransform = default;
	private BoneTransform? nextTransform;

	public GameObjectSelection(int objectTableId)
	{
		this.objectTableId = objectTableId;
	}

	public override bool IsReady => this.isReady;

	public override string Name => this.name ?? "Unknown";
	public override string? Subtitle => null;
	public override bool CanReset => true;

	public override bool LockTransform
	{
		get => this.Services.Pose.AreAllBoneReferencesLocked(this.objectTableId);
		set => this.Services.Pose.SetAllBoneReferencesLocked(this.objectTableId, value);
	}

	public override Transform WorldTransform => this.LocalTransform;

	public override Transform LocalTransform
	{
		get => this.lastTransform;
		set
		{
			this.nextTransform = new();
			this.nextTransform.Translation = value.Translation;
			this.nextTransform.Rotation = value.Rotation;
			this.nextTransform.Scale = value.Scale;
		}
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
			if (this.nextTransform?.Translation != null)
				gameObject->DrawObject->Position = (Vector3)this.nextTransform.Translation;

			if (this.nextTransform?.Rotation != null)
				gameObject->DrawObject->Rotation = (Quaternion)this.nextTransform.Rotation;

			// do not allow objects to scale below 0, it will break the game.
			if (this.nextTransform?.Scale != null)
				gameObject->DrawObject->Scale = Vector3.Max((Vector3)this.nextTransform.Scale, new Vector3(0.1f, 0.1f, 0.1f));

			this.nextTransform = null;
		}

		this.lastTransform = default;
		this.lastTransform.Translation = gameObject->DrawObject->Position;
		this.lastTransform.Rotation = gameObject->DrawObject->Rotation;
		this.lastTransform.Scale = gameObject->DrawObject->Scale;
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

	public override void SetWorldTransform(BoneTransform transform)
	{
		this.nextTransform = transform;
	}
}