namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Data;
using ScreenshotStudio.Library;
using ScreenshotStudio.Structs;

[Sheet("BNpcBase", 0x86278126)]
public class BattleNpc : LibraryExcelRow, IActorAppearance
{
	public byte Battalion { get; protected set; }
	public byte LinkRace { get; protected set; }
	public byte Rank { get; protected set; }
	public float Scale { get; protected set; } = 1.0f;
	public ModelChara? ModelChara { get; protected set; }
	public BattleNpcCustomize? Customize { get; protected set; }
	public NpcEquip? Equipment { get; protected set; }

	public string Key => $"B:{this.RowId.ToString(DataService.NpcNamesIdFormat)}";

	public unsafe override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		// lookup name
		string? name = null;
		if (DataService.NpcNames?.TryGetValue(this.Key, out string? npcNameKey) ?? false)
		{
			if (npcNameKey.StartsWith("N:"))
			{
				uint nameId = uint.Parse(npcNameKey.Substring(2));
				BattleNpcName? npcName = GameDataService.GetRow<BattleNpcName>(nameId);
				if (npcName != null)
				{
					name = npcName.Name;
				}
			}
			else
			{
				name = npcNameKey;
			}
		}

		this.Name = $"Battle NPC #{this.RowId}";

		if (name != null)
		{
			this.Tags.Add("Named");
			this.Name = name;
		}
		else
		{
			this.Tags.Add("Unnamed");
		}

		this.Battalion = parser.ReadColumn<byte>(1);
		this.LinkRace = parser.ReadColumn<byte>(2);
		this.Rank = parser.ReadColumn<byte>(3);
		this.Scale = parser.ReadColumn<float>(4);
		this.ModelChara = parser.ReadRowReference<ushort, ModelChara>(5);
		this.Customize = parser.ReadRowReference<ushort, BattleNpcCustomize>(6);
		this.Equipment = parser.ReadRowReference<ushort, NpcEquip>(7);

		this.Tags.Add("NPC");
	}

	public unsafe void Apply(Actor* actor)
	{
		if (this.Customize != null)
		{
			actor->UpdateCustomize(this.Customize.Customize, Actor.UpdateSource.Library);
		}

		if (this.Equipment != null)
		{
			this.Equipment.Equipment.ApplyToActor(actor);
		}
	}
}
