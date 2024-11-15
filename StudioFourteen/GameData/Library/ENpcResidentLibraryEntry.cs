namespace StudioFourteen.GameData.Library;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using StudioFourteen.Library.Sources;

using ENpcBase = StudioFourteen.GameData.Sheets.ENpcBase;

public class ENpcResidentLibraryEntry(SourceBase source, ENpcResident npc)
	: ExcelLibraryEntry(source, npc.RowId)
{
	public override string Name => npc.Singular.ExtractText();

	public ImageReference? Icon
	{
		get
		{
			CustomizeData? customize = this.ENpcBase?.CustomizeData;
			if (customize == null)
				return null;

			CustomizeData d = customize.Value;
			return d.GetIcon();
		}
	}

	public ENpcBase? ENpcBase => ServiceManager.Instance.GameData.GetRow<ENpcBase>(npc.RowId);
}
