namespace StudioFourteen.Posing;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using StudioFourteen.Plugin;
using StudioFourteen.Structs.Extensions;
using System;
using System.Numerics;

public class GameObjectSelection : TransformSelectionBase
{
	private readonly int objectTableId;
	private string? name;
	private bool isReady = false;

	private Transform lastTransform = default;
	private Transform nextTransform = default;

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

		GameObject* gameObject = (GameObject*)DalamudServices.ObjectTable.GetObjectAddress(this.objectTableId);
		if (gameObject == null || gameObject->DrawObject == null)
			return;

		this.name = gameObject->GetNameAsString();

		if (this.nextTransform.Translation != Vector3.Zero)
		{
			gameObject->DrawObject->Position = this.nextTransform.Translation;
			this.nextTransform.Translation = Vector3.Zero;
		}

		if (this.nextTransform.Rotation != Quaternion.Identity && this.nextTransform.Rotation != Quaternion.Zero)
		{
			gameObject->DrawObject->Rotation = this.nextTransform.Rotation;
			this.nextTransform.Rotation = Quaternion.Identity;
		}

		if (this.nextTransform.Scale != Vector3.Zero)
		{
			gameObject->DrawObject->Scale = this.nextTransform.Scale;
			this.nextTransform.Scale = Vector3.Zero;
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
}