namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Tags;
using System;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;
using System.Text;

public class EquipSlotCategory : Lumina.Excel.GeneratedSheets.EquipSlotCategory
{
	public TagCollection Tags { get; init; } = new();
	public string? Name { get; private set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		StringBuilder nameBuilder = new();
		foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
		{
			nameBuilder.Append(slot.GetDisplayName());
			nameBuilder.Append(" ");
		}

		foreach (WeaponSlot slot in Enum.GetValues<WeaponSlot>())
		{
			nameBuilder.Append(slot.GetDisplayName());
			nameBuilder.Append(" ");
		}

		this.Name = nameBuilder.ToString();
	}

	public bool Contains(WeaponSlot slot)
	{
		switch (slot)
		{
			case WeaponSlot.MainHand: return this.MainHand == 1;
			case WeaponSlot.OffHand: return this.OffHand == 1;
			case WeaponSlot.Unk: return false;
		}

		throw new Exception($"Invalid Weapon Slot: {slot}");
	}

	public bool Contains(EquipmentSlot slot)
	{
		switch (slot)
		{
			case EquipmentSlot.Head: return this.Head == 1;
			case EquipmentSlot.Body: return this.Body == 1;
			case EquipmentSlot.Hands: return this.Gloves == 1;
			case EquipmentSlot.Legs: return this.Legs == 1;
			case EquipmentSlot.Feet: return this.Feet == 1;
			case EquipmentSlot.Ears: return this.Ears == 1;
			case EquipmentSlot.Neck: return this.Neck == 1;
			case EquipmentSlot.Wrists: return this.Wrists == 1;
			case EquipmentSlot.RFinger: return this.FingerR == 1;
			case EquipmentSlot.LFinger: return this.FingerL == 1;
		}

		throw new Exception($"Invalid Equipment Slot: {slot}");
	}

	public TagCollection ToTags()
	{
		TagCollection tags = new();

		foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
		{
			if (this.Contains(slot))
			{
				tags.Add(slot.ToTag());
			}
		}

		foreach (WeaponSlot slot in Enum.GetValues<WeaponSlot>())
		{
			if (this.Contains(slot))
			{
				tags.Add(slot.ToTag());
			}
		}

		return tags;
	}
}