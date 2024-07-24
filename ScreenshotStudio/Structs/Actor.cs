// Ktisis
// https://github.com/ktisis-tools/Ktisis/
// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/Actor.cs
// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/ActorDrawData.cs

// Anamnesis
// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Memory/ActorCustomizeMemory.cs
// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Memory/ActorBasicMemory.cs

// https://github.com/aers/FFXIVClientStructs/blob/main/FFXIVClientStructs/FFXIV/Client/Game/Character/Character.cs
// https://github.com/aers/FFXIVClientStructs/blob/main/FFXIVClientStructs/FFXIV/Client/Game/Object/GameObject.cs
namespace ScreenshotStudio.Structs;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using System;
using System.Runtime.InteropServices;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public enum RenderMode : uint
{
	Draw = 0,
	Unload = 2,
	Load = 4,
}

[Flags]
public enum ActorFlags : byte
{
	None = 0,
	WeaponsVisible = 1,
	WeaponsDrawn = 2,
	VisorToggle = 8,
}

[StructLayout(LayoutKind.Explicit, Size = 0x84A)]
public struct Actor
{
	[FieldOffset(0)] public GameObject GameObject;

	[FieldOffset(0x1AC)] public uint ModelCharaRowId;
	[FieldOffset(0x708)] public DrawDataContainer DrawData;
	[FieldOffset(0x89E)] public bool IsHatHidden;

	public enum UpdateSource
	{
		Interface,
		Library,
		Restore,
	}

	public unsafe string? Name
	{
		get
		{
			fixed (byte* ptr = this.GameObject.Name)
			{
				return ptr == null ? null : Marshal.PtrToStringUTF8((IntPtr)ptr);
			}
		}
	}

	public void UpdateModel(ModelChara modelChara, UpdateSource source, bool apply = true)
	{
		this.UpdateModel(modelChara.RowId, source, apply);
	}

	public void UpdateModel(uint modelCharaRowId, UpdateSource source, bool apply = true)
	{
		if (source != UpdateSource.Restore)
			ServiceManager.Instance.ActorAppearanceBackup.Backup(this);

		this.ModelCharaRowId = modelCharaRowId;

		if (apply)
		{
			this.UpdateCustomizeInternal(true, source);
		}
	}

	public byte GetCustomizeValue(CustomizeIndex option)
	{
		return this.DrawData.CustomizeData.GetValue(option);
	}

	public bool SetCustomizeValue(CustomizeIndex option, byte value, UpdateSource source, bool apply = true)
	{
		if (source != UpdateSource.Restore)
			ServiceManager.Instance.ActorAppearanceBackup.Backup(this);

		this.DrawData.CustomizeData.SetValue(option, value);

		bool needsRedraw = option == CustomizeIndex.Race;
		needsRedraw |= option == CustomizeIndex.Tribe;
		needsRedraw |= option == CustomizeIndex.ModelType;
		needsRedraw |= option == CustomizeIndex.Gender;

		if (apply)
		{
			this.UpdateCustomizeInternal(needsRedraw, source);
		}

		return needsRedraw;
	}

	public unsafe void UpdateWeapon(WeaponSlot slot, WeaponModelId item, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		if (source != UpdateSource.Restore)
			ServiceManager.Instance.ActorAppearanceBackup.Backup(this);

		fixed (DrawDataContainer* drawData = &this.DrawData)
		{
			this.DrawData.LoadWeapon(slot, item, 1, 1, 0, 0);
		}
	}

	public unsafe void UpdateEquipment(Span<EquipmentModelId> equipment, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		if (source != UpdateSource.Restore)
			ServiceManager.Instance.ActorAppearanceBackup.Backup(this);

		for (int i = 0; i < equipment.Length; i++)
		{
			EquipmentSlot slot = (EquipmentSlot)i;
			this.UpdateEquipment(slot, equipment[i], source);
		}
	}

	public unsafe void UpdateEquipment(EquipmentSlot slot, EquipmentModelId item, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		if (source != UpdateSource.Restore)
			ServiceManager.Instance.ActorAppearanceBackup.Backup(this);

		fixed (DrawDataContainer* drawData = &this.DrawData)
		{
			this.DrawData.LoadEquipment(slot, &item, true);
		}
	}

	public unsafe void UpdateCustomize(bool redraw, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();
		this.UpdateCustomize(this.DrawData.CustomizeData, redraw, source);
	}

	public unsafe void UpdateCustomize(CustomizeData customize, bool redraw, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		if (source != UpdateSource.Restore)
			ServiceManager.Instance.ActorAppearanceBackup.Backup(this);

		this.DrawData.CustomizeData.Import(customize);
		this.UpdateCustomizeInternal(redraw, source);
	}

	public unsafe void UpdateCustomizeInternal(bool redraw, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		if (!redraw)
		{
			fixed (CustomizeData* custom = &this.DrawData.CustomizeData)
			{
				redraw |= ((Human*)this.GameObject.DrawObject)->UpdateDrawData((byte*)custom, true) == false;
			}
		}

		if (redraw)
		{
			this.GameObject.DisableDraw();
			this.GameObject.EnableDraw();
		}
	}
}