// Ktisis
// https://github.com/ktisis-tools/Ktisis/
// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/Actor.cs
// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/ActorDrawData.cs

// Anamnesis
// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Memory/ActorCustomizeMemory.cs
// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Memory/ActorBasicMemory.cs
namespace ScreenshotStudio.Structs;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.Utilities;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 0x84A)]
public struct Actor
{
	[FieldOffset(0)] public GameObject GameObject;
	[FieldOffset(0x88)] public byte ObjectID;
	[FieldOffset(0x100)] public unsafe ActorModel* Model;
	[FieldOffset(0x118)] public RenderMode RenderMode;
	[FieldOffset(0x1AC)] public uint ModelCharaRowId;
	[FieldOffset(0x708)] public ActorDrawData DrawData;
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
			this.UpdateCustomize(true, source);
		}
	}

	public byte GetCustomizeValue(CustomizeIndex option)
	{
		return this.DrawData.Customize.GetValue(option);
	}

	public bool SetCustomizeValue(CustomizeIndex option, byte value, UpdateSource source, bool apply = true)
	{
		if (source != UpdateSource.Restore)
			ServiceManager.Instance.ActorAppearanceBackup.Backup(this);

		this.DrawData.Customize.SetValue(option, value);

		bool needsRedraw = option == CustomizeIndex.Race;
		needsRedraw |= option == CustomizeIndex.Tribe;
		needsRedraw |= option == CustomizeIndex.ModelType;
		needsRedraw |= option == CustomizeIndex.Gender;

		if (apply)
		{
			this.UpdateCustomize(needsRedraw, source);
		}

		return needsRedraw;
	}

	public unsafe void UpdateCustomize(Customize customize, UpdateSource source, bool redraw = false)
	{
		Threads.RunOnFrameworkThread(this, (p) => ((Actor*)p)->UpdateCustomizeInternal(customize, redraw, source));
	}

	public unsafe void UpdateCustomize(bool redraw, UpdateSource source)
	{
		Threads.RunOnFrameworkThread(this, (p) => ((Actor*)p)->UpdateCustomizeInternal(redraw, source));
	}

	public unsafe void UpdateEquipment(Equipment equipment, UpdateSource source)
	{
		if (source != UpdateSource.Restore)
			ServiceManager.Instance.ActorAppearanceBackup.Backup(this);

		fixed (ActorDrawData* drawData = &this.DrawData)
		{
			for (int i = 0; i < (int)Equipment.EquipIndex.Count; i++)
			{
				Equipment.EquipIndex index = (Equipment.EquipIndex)i;
				ActorDrawDataExtensions.ChangeEquip(drawData, index, equipment.GetItem(index), true);
			}
		}
	}

	public unsafe void UpdateEquipment(Equipment.EquipIndex index, ItemEquip item, UpdateSource source)
	{
		if (source != UpdateSource.Restore)
			ServiceManager.Instance.ActorAppearanceBackup.Backup(this);

		fixed (ActorDrawData* drawData = &this.DrawData)
		{
			ActorDrawDataExtensions.ChangeEquip(drawData, index, (ItemEquip)item, true);
		}
	}

	private unsafe void UpdateCustomizeInternal(Customize customize, bool redraw, UpdateSource source)
	{
		if (source != UpdateSource.Restore)
			ServiceManager.Instance.ActorAppearanceBackup.Backup(this);

		Threads.VerifyFrameworkThread();

		this.DrawData.Customize.Import(customize);
		this.UpdateCustomizeInternal(redraw, source);
	}

	private unsafe void UpdateCustomizeInternal(bool redraw, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		if (!redraw)
		{
			fixed (Customize* custom = &this.DrawData.Customize)
			{
				redraw |= ((Human*)this.Model)->UpdateDrawData((byte*)custom, true) == false;
			}
		}

		if (redraw)
		{
			this.GameObject.DisableDraw();
			this.GameObject.EnableDraw();
		}
	}
}

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