namespace StudioFourteen.GameData.Library;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using StudioFourteen.Library;
using StudioFourteen.Library.Sources;

using BNpcCustomize = StudioFourteen.GameData.Sheets.BNpcCustomize;

public class BNpcBaseLibraryEntry(SourceBase source, BNpcBase npc)
	: ExcelLibraryEntry(source, npc.RowId)
{
	public string? Description => null;

	public override string Name
	{
		get
		{
			if (GameDataService.BattleNpcNameIndex.TryGetValue($"{npc.RowId}", out int nameRowId))
			{
				BNpcName? name = ServiceManager.Instance.GameData.GetRow<BNpcName>(nameRowId);
				if (name != null)
				{
					return name.Value.Singular.ExtractText();
				}
			}

			return $"#{npc.RowId}";
		}
	}

	public ImageReference? Icon
	{
		get
		{
			CustomizeData? customize = this.Customize;
			if (customize == null)
				return null;

			CustomizeData d = customize.Value;
			return d.GetIcon();
		}
	}

	public CustomizeData? Customize
	{
		get
		{
			BNpcCustomize? customize = ServiceManager.Instance.GameData.GetRow<BNpcCustomize>(npc.BNpcCustomize.RowId);
			return customize?.Data;
		}
	}

	protected override string GetInternalId() => $"BNpcBase_{npc.RowId}";
}
