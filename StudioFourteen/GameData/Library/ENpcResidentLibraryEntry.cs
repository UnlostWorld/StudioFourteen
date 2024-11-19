namespace StudioFourteen.GameData.Library;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FontAwesome.Sharp;
using Lumina.Excel.Sheets;
using StudioFourteen.Appearance;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Library.Sources;
using System.Threading.Tasks;

using ENpcBase = StudioFourteen.GameData.Sheets.ENpcBase;

public class ENpcResidentLibraryEntry(SourceBase source, ENpcResident npc)
	: ExcelLibraryEntry(source, npc.RowId), ICharacterAppearance
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

	[LibraryMenu(IconChar.Plus, "LOC_AppearanceCreateCharacter")]
	public Task Spawn()
	{
		return ServiceManager.Instance.CharacterLifecycle.CreateAsync(this);
	}

	[LibraryMenuTarget(IconChar.UserShield, "LOC_AppearanceApplyTo")]
	public Task Apply(int objectTableIndex)
	{
		////throw new NotImplementedException();
		return Task.CompletedTask;
	}
}
