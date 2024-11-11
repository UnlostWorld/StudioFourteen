namespace StudioFourteen.GameData;

using Lumina;
using Lumina.Excel;

public static class GameDataExtensions
{
	public static T? GetRow<T>(this GameData gameData, int rowIndex)
		where T : ExcelRow
	{
		return gameData.GetRow<T>((uint)rowIndex);
	}

	public static T? GetRow<T>(this GameData gameData, uint rowIndex)
		where T : ExcelRow
	{
		ExcelSheet<T>? sheet = gameData.GetExcelSheet<T>();
		if (sheet == null)
			return null;

		return sheet.GetRow(rowIndex);
	}
}
