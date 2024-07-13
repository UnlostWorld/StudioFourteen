namespace ScreenshotStudio.GameData.Excel;

using Lumina;
using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Structs;
using System.Text;
using static ScreenshotStudio.Structs.Equipment;
using ScreenshotStudio.Structs.Extensions;

[Sheet("NpcEquip", 0xe91c87ba)]
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

	public ItemEquip Head { get; protected set; }
	public bool Visor { get; protected set; } = true;
	public ItemEquip Body { get; protected set; }
	public ItemEquip Legs { get; protected set; }
	public ItemEquip Feet { get; protected set; }
	public ItemEquip Hands { get; protected set; }
	public ItemEquip Wrists { get; protected set; }
	public ItemEquip Neck { get; protected set; }
	public ItemEquip Ears { get; protected set; }
	public ItemEquip LeftRing { get; protected set; }
	public ItemEquip RightRing { get; protected set; }

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

	public unsafe void ApplyToActor(Actor* actor, NpcEquipment? fallback = null)
	{
		this.ApplyToActor(actor, EquipIndex.Head, this.Head, fallback?.Head);
		this.ApplyToActor(actor, EquipIndex.Chest, this.Body, fallback?.Body);
		this.ApplyToActor(actor, EquipIndex.Hands, this.Hands, fallback?.Hands);
		this.ApplyToActor(actor, EquipIndex.Legs, this.Legs, fallback?.Legs);
		this.ApplyToActor(actor, EquipIndex.Feet, this.Feet, fallback?.Feet);
		this.ApplyToActor(actor, EquipIndex.Earring, this.Ears, fallback?.Ears);
		this.ApplyToActor(actor, EquipIndex.Necklace, this.Neck, fallback?.Neck);
		this.ApplyToActor(actor, EquipIndex.Bracelet, this.Wrists, fallback?.Wrists);
		this.ApplyToActor(actor, EquipIndex.RingRight, this.RightRing, fallback?.RightRing);
		this.ApplyToActor(actor, EquipIndex.RingLeft, this.LeftRing, fallback?.LeftRing);
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

	private unsafe void ApplyToActor(Actor* actor, EquipIndex index, ItemEquip a, ItemEquip? b)
	{
		ItemEquip toUse = a;
		if (b != null && b.Value.Base > 0)
			toUse = b.Value;

		actor->UpdateEquipment(index, toUse, Actor.UpdateSource.Library);
	}

	private ItemEquip ParseItem(RowParser parser, int column)
	{
		ItemEquip eq = default;

		var model = parser.ReadColumn<uint>(column + 0);

		short b = (short)model;

		if (b > ushort.MinValue)
			eq.Base = (ushort)b;

		short variant = (short)(model >> 16);
		if (variant >= byte.MinValue && variant < byte.MaxValue)
			eq.Variant = (byte)variant;

		eq.Dye1 = parser.ReadColumn<byte>(column + 1);
		eq.Dye2 = parser.ReadColumn<byte>(column + 2);
		return eq;
	}

	private void AddToString(object? val, StringBuilder builder)
	{
		if (val == null)
		{
			builder.Append("_");
		}
		else if (val is ItemEquip eq)
		{
			builder.Append(eq.Base);
			builder.Append(eq.Variant);
			builder.Append(eq.Dye1);
			builder.Append(eq.Dye2);
		}
		else
		{
			builder.Append(val);
		}
	}
}