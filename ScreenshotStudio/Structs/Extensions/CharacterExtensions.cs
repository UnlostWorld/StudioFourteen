namespace FFXIVClientStructs.FFXIV.Client.Game.Character;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using global::System;
using global::System.Runtime.InteropServices;
using ScreenshotStudio;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.Utilities;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public enum RenderMode : uint
{
	Draw = 0,
	Unload = 2,
	Load = 4,
}

[Flags]
public enum CharacterFlags : byte
{
	None = 0,
	WeaponsVisible = 1,
	WeaponsDrawn = 2,
	VisorToggle = 8,
}

public static class CharacterExtensions
{
	public enum UpdateSource
	{
		Interface,
		Library,
		Restore,
	}

	public static bool CanDraw(ref this Character self)
	{
		if (!self.IsReadyToDraw())
			return false;

		return self.RenderFlags == (int)RenderMode.Draw;
	}

	public static unsafe string? GetNameAsString(ref this Character self)
	{
		fixed (byte* ptr = self.Name)
		{
			return ptr == null ? null : Marshal.PtrToStringUTF8((IntPtr)ptr);
		}
	}

	public static unsafe CharacterBase* GetCharacterBase(ref this Character self)
	{
		return (CharacterBase*)self.DrawObject;
	}

	public static void UpdateModel(ref this Character self, ModelChara modelChara, UpdateSource source, bool apply = true)
	{
		self.UpdateModel((int)modelChara.RowId, source, apply);
	}

	public static void UpdateModel(ref this Character self, int modelCharaId, UpdateSource source, bool apply = true)
	{
		if (source != UpdateSource.Restore)
			ServiceManager.Instance.CharacterAppearanceBackup.Backup(self);

		self.ModelCharaId = modelCharaId;

		if (apply)
		{
			self.UpdateCustomizeInternal(true, source);
		}
	}

	public static byte GetCustomizeValue(ref this Character self, CustomizeIndex option)
	{
		return self.DrawData.CustomizeData.GetValue(option);
	}

	public static bool SetCustomizeValue(ref this Character self, CustomizeIndex option, byte value, UpdateSource source, bool apply = true)
	{
		if (source != UpdateSource.Restore)
			ServiceManager.Instance.CharacterAppearanceBackup.Backup(self);

		self.DrawData.CustomizeData.SetValue(option, value);

		bool needsRedraw = option == CustomizeIndex.Race;
		needsRedraw |= option == CustomizeIndex.Tribe;
		needsRedraw |= option == CustomizeIndex.ModelType;
		needsRedraw |= option == CustomizeIndex.Gender;

		if (apply)
		{
			self.UpdateCustomizeInternal(needsRedraw, source);
		}

		return needsRedraw;
	}

	public static unsafe void UpdateWeapon(ref this Character self, WeaponSlot slot, WeaponModelId item, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		if (source != UpdateSource.Restore)
			ServiceManager.Instance.CharacterAppearanceBackup.Backup(self);

		fixed (DrawDataContainer* drawData = &self.DrawData)
		{
			drawData->LoadWeapon(slot, item, 1, 1, 0, 0);
		}
	}

	public static unsafe void UpdateEquipment(ref this Character self, Span<EquipmentModelId> equipment, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		if (source != UpdateSource.Restore)
			ServiceManager.Instance.CharacterAppearanceBackup.Backup(self);

		for (int i = 0; i < equipment.Length; i++)
		{
			EquipmentSlot slot = (EquipmentSlot)i;
			self.UpdateEquipment(slot, equipment[i], source);
		}
	}

	public static unsafe void UpdateEquipment(ref this Character self, EquipmentSlot slot, EquipmentModelId item, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		if (source != UpdateSource.Restore)
			ServiceManager.Instance.CharacterAppearanceBackup.Backup(self);

		fixed (DrawDataContainer* drawData = &self.DrawData)
		{
			drawData->LoadEquipment(slot, &item, true);
		}
	}

	public static unsafe void UpdateCustomize(ref this Character self, bool redraw, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();
		self.UpdateCustomize(self.DrawData.CustomizeData, redraw, source);
	}

	public static unsafe void UpdateCustomize(ref this Character self, CustomizeData customize, bool redraw, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		if (source != UpdateSource.Restore)
			ServiceManager.Instance.CharacterAppearanceBackup.Backup(self);

		self.DrawData.CustomizeData.Import(customize);
		self.UpdateCustomizeInternal(redraw, source);
	}

	public static unsafe void UpdateCustomizeInternal(ref this Character self, bool redraw, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		if (!redraw)
		{
			fixed (CustomizeData* custom = &self.DrawData.CustomizeData)
			{
				redraw |= ((Human*)self.DrawObject)->UpdateDrawData((byte*)custom, true) == false;
			}
		}

		if (redraw)
		{
			self.DisableDraw();
			self.EnableDraw();
		}
	}
}