namespace FFXIVClientStructs.FFXIV.Client.Game.Character;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using global::System;
using Lumina.Excel.Sheets;
using StudioFourteen;
using StudioFourteen.GameData.Sheets;
using StudioFourteen.Utilities;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;
using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

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

	public static unsafe string? GetRoleOrDisplayName(ref this Character self)
	{
		string? nickname = ServiceManager.Instance.Roles.GetRole(self.ObjectIndex);
		if (nickname != null)
			return nickname;

		return self.GetDisplayName();
	}

	public static unsafe void SetDisplayName(ref this Character self, string displayName)
	{
		self.GameObject.SetDisplayName(displayName);
	}

	public static unsafe string GetDisplayName(ref this Character self)
	{
		string selfName = self.GameObject.GetDisplayName();

		if (self.CompanionOwnerId > 0)
		{
			Character* pOwner = (Character*)CharacterManager.Instance()->LookupBattleCharaByEntityId(self.CompanionOwnerId);
			if (pOwner != null)
			{
				string? ownersName = pOwner->GetDisplayName();
				if (ownersName != null)
				{
					ownersName = ownersName.Split(' ')[0];
					return $"{ownersName}'s {selfName}";
				}
			}
		}

		return selfName;
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
			ServiceManager.Instance.CharacterAppearance.Backup(self);

		self.ModelContainer.ModelCharaId = modelCharaId;

		if (apply)
		{
			self.UpdateCustomizeInternal(true, source);
		}
	}

	public static CharaMakeType? GetCharaMakeType(ref readonly this Character self)
	{
		return self.DrawData.CustomizeData.GetMakeType();
	}

	public static byte GetCustomizeValue(ref readonly this Character self, CustomizeIndex option)
	{
		return self.DrawData.CustomizeData.GetValue(option);
	}

	public static bool SetCustomizeValue(ref this Character self, CustomizeIndex option, byte value, UpdateSource source, bool apply = true)
	{
		if (source != UpdateSource.Restore)
			ServiceManager.Instance.CharacterAppearance.Backup(self);

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
			ServiceManager.Instance.CharacterAppearance.Backup(self);

		fixed (DrawDataContainer* drawData = &self.DrawData)
		{
			drawData->LoadWeapon(slot, item, 1, 1, 0, 0);
		}
	}

	public static unsafe void UpdateEquipment(ref this Character self, Span<EquipmentModelId> equipment, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		if (source != UpdateSource.Restore)
			ServiceManager.Instance.CharacterAppearance.Backup(self);

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
			ServiceManager.Instance.CharacterAppearance.Backup(self);

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
			ServiceManager.Instance.CharacterAppearance.Backup(self);

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

	public static ObjectKind GetKind(ref this Character self)
	{
		return (ObjectKind)self.ObjectKind;
	}
}