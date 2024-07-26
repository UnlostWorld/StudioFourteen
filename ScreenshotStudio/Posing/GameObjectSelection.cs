namespace ScreenshotStudio.Posing;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ScreenshotStudio.Plugin;
using System;
using System.Numerics;

public class GameObjectSelection : SelectionBase
{
	private readonly ushort objectTableId;
	private string? name;

	private Vector3 lastTranslation;
	private Vector3 nextTranslation;

	private Quaternion lastRotation = Quaternion.Identity;
	private Quaternion nextRotation = Quaternion.Identity;

	private Vector3 lastScale;
	private Vector3 nextScale;

	public GameObjectSelection(ushort objectTableId)
	{
		this.objectTableId = objectTableId;
	}

	public override string Name => this.name ?? "Unknown";
	public override bool LockTransform { get; set; }

	public override Vector3 Translation
	{
		get => this.lastTranslation;
		set => this.nextTranslation = value;
	}

	public override Quaternion Rotation
	{
		get => this.lastRotation;
		set => this.nextRotation = value;
	}

	public override Vector3 Scale
	{
		get => this.lastScale;
		set => this.nextScale = value;
	}

	public unsafe override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		if (DalamudServices.ObjectTable == null)
			throw new Exception("No Object Table");

		GameObject* gameObject = (GameObject*)DalamudServices.ObjectTable.GetObjectAddress(this.objectTableId);

		this.name = gameObject->GetNameAsString();

		if (this.nextTranslation != Vector3.Zero)
		{
			gameObject->DrawObject->Position = this.nextTranslation;
			this.nextTranslation = Vector3.Zero;
		}

		if (this.nextRotation != Quaternion.Identity)
		{
			gameObject->DrawObject->Rotation = this.nextRotation;
			this.nextRotation = Quaternion.Identity;
		}

		if (this.nextScale != Vector3.Zero)
		{
			gameObject->DrawObject->Scale = this.nextScale;
			this.nextScale = Vector3.Zero;
		}

		this.lastTranslation = gameObject->DrawObject->Position;
		this.lastRotation = gameObject->DrawObject->Rotation;
		this.lastScale = gameObject->DrawObject->Scale;
	}
}