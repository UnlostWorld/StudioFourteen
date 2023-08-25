// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Tags;
using XivToolsWpf;

[Sheet("Item", 0x800968c9)]
public class Item : LibraryExcelRow
{
	public string Name { get; protected set; } = string.Empty;
	public string Description { get; protected set; } = string.Empty;
	public ImageReference? Icon { get; protected set; }
	public byte EquipLevel { get; protected set; }
	public ushort ModelSet { get; protected set; }
	public ushort ModelBase { get; protected set; }
	public ushort ModelVariant { get; protected set; }
	public ushort SubModelSet { get; protected set; }
	public ushort SubModelBase { get; protected set; }
	public ushort SubModelVariant { get; protected set; }
	public ClassJobCategory? ClassJobs { get; protected set; }
	public EquipSlotCategory? EquipSlot { get; protected set; }
	public EquipRaceCategory? EquipRestriction { get; protected set; }
	public bool HasSubModel => this.SubModelSet != 0;

	public override bool Search(string[]? query)
	{
		if (SearchUtility.Matches(this.Name, query))
			return true;

		if (SearchUtility.Matches(this.Description, query))
			return true;

		return base.Search(query);
	}

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Description = parser.ReadColumn<SeString>(8) ?? string.Empty;
		this.Name = parser.ReadColumn<SeString>(9) ?? string.Empty;
		this.Icon = parser.ReadImageReference<ushort>(10);
		////ItemLevel? itemLevel = parser.ReadRowReference<ushort, ItemLevel>(11);

		this.EquipSlot = parser.ReadRowReference<byte, EquipSlotCategory>(17);
		this.EquipLevel = parser.ReadColumn<byte>(40);
		this.EquipRestriction = parser.ReadRowReference<byte, EquipRaceCategory>(42);
		this.ClassJobs = parser.ReadRowReference<byte, ClassJobCategory>(43);

		bool isWeapon = this.FitsInSlot(ItemSlots.MainHand) || this.FitsInSlot(ItemSlots.OffHand);

		ulong mainModel = parser.ReadColumn<ulong>(47);
		ulong subModel = parser.ReadColumn<ulong>(48);
		if (isWeapon)
		{
			this.ModelSet = (ushort)mainModel;
			this.ModelBase = (ushort)(mainModel >> 16);
			this.ModelVariant = (ushort)(mainModel >> 32);

			this.SubModelSet = (ushort)subModel;
			this.SubModelBase = (ushort)(subModel >> 16);
			this.SubModelVariant = (ushort)(subModel >> 32);
		}
		else
		{
			this.ModelSet = 0;
			this.ModelBase = (ushort)mainModel;
			this.ModelVariant = (ushort)(mainModel >> 16);

			this.SubModelSet = 0;
			this.SubModelBase = (ushort)subModel;
			this.SubModelVariant = (ushort)(subModel >> 16);
		}

		if (this.EquipSlot != null)
			this.Tags.AddRange(this.EquipSlot.ToTags());

		if (this.EquipRestriction != null)
			this.Tags.AddRange(this.EquipRestriction.ToTags());

		if (this.ClassJobs != null)
			this.Tags.AddRange(this.ClassJobs.ToTags());

		if (string.IsNullOrEmpty(this.Name))
		{
			this.Tags.Add("Unnamed");
		}

		if (this.EquipLevel <= 1)
		{
			this.Tags.Add("No Level");
		}
	}

	public virtual bool FitsInSlot(ItemSlots slot)
	{
		return this.EquipSlot?.Contains(slot) ?? false;
	}

	public virtual bool IsItemEquip(ItemEquip item)
	{
		if (this.ModelSet == 0 && this.ModelBase == item.Base && this.ModelVariant == item.Variant)
			return true;

		if (this.SubModelSet == 0 && this.SubModelBase == item.Base && this.SubModelVariant == item.Variant)
			return true;

		return false;
	}
}
