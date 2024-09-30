namespace ScreenshotStudio.GameData.Excel;

using Lumina;
using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Structs;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

[Sheet("NpcEquip", 0x4004f596)]
public class NpcEquip : StudioExcelRow
{
	public NpcEquipment Equipment { get; init; } = new();

	public override void PopulateData(RowParser parser, GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Equipment.Parse(parser, 0);
	}
}

public class NpcEquipment
{
	public ulong MainHand { get; protected set; }
	public byte DyeMainHand { get; protected set; }
	public byte Dye2MainHand { get; protected set; }

	public ulong OffHand { get; protected set; }
	public byte DyeOffHand { get; protected set; }
	public byte Dye2OffHand { get; protected set; }

	public EquipmentModelId Head { get; protected set; }
	public bool Visor { get; protected set; } = true;
	public EquipmentModelId Body { get; protected set; }
	public EquipmentModelId Legs { get; protected set; }
	public EquipmentModelId Feet { get; protected set; }
	public EquipmentModelId Hands { get; protected set; }
	public EquipmentModelId Wrists { get; protected set; }
	public EquipmentModelId Neck { get; protected set; }
	public EquipmentModelId Ears { get; protected set; }
	public EquipmentModelId LeftRing { get; protected set; }
	public EquipmentModelId RightRing { get; protected set; }

	public void Parse(RowParser parser, int startColumn)
	{
		this.MainHand = parser.ReadColumn<ulong>(startColumn + 0);
		this.DyeMainHand = parser.ReadColumn<byte>(startColumn + 1);
		this.Dye2MainHand = parser.ReadColumn<byte>(startColumn + 2);

		this.OffHand = parser.ReadColumn<ulong>(startColumn + 3);
		this.DyeOffHand = parser.ReadColumn<byte>(startColumn + 4);
		this.Dye2OffHand = parser.ReadColumn<byte>(startColumn + 5);

		this.Head = this.ParseItem(parser, startColumn + 6);
		this.Visor = parser.ReadColumn<bool>(startColumn + 9);
		this.Body = this.ParseItem(parser, startColumn + 10);
		this.Hands = this.ParseItem(parser, startColumn + 13);
		this.Legs = this.ParseItem(parser, startColumn + 16);
		this.Feet = this.ParseItem(parser, startColumn + 19);
		this.Ears = this.ParseItem(parser, startColumn + 22);
		this.Neck = this.ParseItem(parser, startColumn + 25);
		this.Wrists = this.ParseItem(parser, startColumn + 28);
		this.LeftRing = this.ParseItem(parser, startColumn + 31);
		this.RightRing = this.ParseItem(parser, startColumn + 34);
	}

	public unsafe void ApplyTo(Character* character, NpcEquipment? fallback = null)
	{
		this.ApplyTo(character, EquipmentSlot.Head, this.Head, fallback?.Head);
		this.ApplyTo(character, EquipmentSlot.Body, this.Body, fallback?.Body);
		this.ApplyTo(character, EquipmentSlot.Hands, this.Hands, fallback?.Hands);
		this.ApplyTo(character, EquipmentSlot.Legs, this.Legs, fallback?.Legs);
		this.ApplyTo(character, EquipmentSlot.Feet, this.Feet, fallback?.Feet);
		this.ApplyTo(character, EquipmentSlot.Ears, this.Ears, fallback?.Ears);
		this.ApplyTo(character, EquipmentSlot.Neck, this.Neck, fallback?.Neck);
		this.ApplyTo(character, EquipmentSlot.Wrists, this.Wrists, fallback?.Wrists);
		this.ApplyTo(character, EquipmentSlot.RFinger, this.RightRing, fallback?.RightRing);
		this.ApplyTo(character, EquipmentSlot.LFinger, this.LeftRing, fallback?.LeftRing);
	}

	public void GetStringForHash(StringBuilder sb)
	{
		this.AddToString(this.MainHand, sb);
		this.AddToString(this.DyeMainHand, sb);
		this.AddToString(this.Dye2MainHand, sb);
		this.AddToString(this.OffHand, sb);
		this.AddToString(this.DyeOffHand, sb);
		this.AddToString(this.Dye2OffHand, sb);
		this.AddToString(this.Head, sb);
		this.AddToString(this.Body, sb);
		this.AddToString(this.Legs, sb);
		this.AddToString(this.Feet, sb);
		this.AddToString(this.Hands, sb);
		this.AddToString(this.Wrists, sb);
		this.AddToString(this.Neck, sb);
		this.AddToString(this.Ears, sb);
		this.AddToString(this.LeftRing, sb);
		this.AddToString(this.RightRing, sb);
	}

	private unsafe void ApplyTo(Character* character, EquipmentSlot index, EquipmentModelId a, EquipmentModelId? b)
	{
		EquipmentModelId toUse = a;
		if (b != null && b.Value.Id > 0)
			toUse = b.Value;

		character->UpdateEquipment(index, toUse, CharacterExtensions.UpdateSource.Library);
	}

	private EquipmentModelId ParseItem(RowParser parser, int column)
	{
		EquipmentModelId eq = default;

		var model = parser.ReadColumn<uint>(column + 0);

		short b = (short)model;

		if (b > ushort.MinValue)
			eq.Id = (ushort)b;

		short variant = (short)(model >> 16);
		if (variant >= byte.MinValue && variant < byte.MaxValue)
			eq.Variant = (byte)variant;

		eq.Stain0 = parser.ReadColumn<byte>(column + 1);
		eq.Stain1 = parser.ReadColumn<byte>(column + 2);
		return eq;
	}

	private void AddToString(object? val, StringBuilder builder)
	{
		if (val == null)
		{
			builder.Append("_");
		}
		else if (val is EquipmentModelId eq)
		{
			builder.Append(eq.Id);
			builder.Append(eq.Variant);
			builder.Append(eq.Stain0);
			builder.Append(eq.Stain1);
		}
		else
		{
			builder.Append(val);
		}
	}
}