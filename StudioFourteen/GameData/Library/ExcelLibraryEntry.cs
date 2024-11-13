namespace StudioFourteen.GameData.Library;

using StudioFourteen.Library;
using StudioFourteen.Library.Sources;

public abstract class ExcelLibraryEntry(SourceBase source, uint rowId)
	: LibraryEntryBase(source)
{
	public uint RowId => rowId;
}
