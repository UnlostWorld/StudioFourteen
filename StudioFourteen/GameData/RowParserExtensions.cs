namespace StudioFourteen.GameData;

using System;
using Lumina.Excel;
using Lumina.Text;

public static class RowParserExtensions
{
	public static ImageReference? ReadImageReference<TColumn>(this RowParser self, int column)
	{
		TColumn? id = self.ReadColumn<TColumn>(column);
		return CreateImageReference<TColumn>(id);
	}

	public static ImageReference? ReadImageReferenceOffset<TColumn>(this RowParser self, int offset)
	{
		TColumn? id = self.ReadOffset<TColumn>(offset);
		return CreateImageReference<TColumn>(id);
	}

	public static string? ReadStringOffset(this RowParser self, ushort offset)
	{
		SeString? value = self.ReadOffset<SeString>(offset);
		if (value == null)
			return null;

		if (string.IsNullOrEmpty(value.RawString) || string.IsNullOrWhiteSpace(value.RawString))
			return null;

		return value.RawString;
	}

	public static string? ReadString(this RowParser self, int column)
	{
		SeString? value = self.ReadColumn<SeString>(column);
		if (value == null)
			return null;

		if (string.IsNullOrEmpty(value.RawString) || string.IsNullOrWhiteSpace(value.RawString))
			return null;

		return value.RawString;
	}

	public static TRow? ReadRowReference<TColumn, TRow>(this RowParser self, int column, int minValue = int.MinValue)
		where TRow : Lumina.Excel.ExcelRow
	{
		TColumn? id = self.ReadColumn<TColumn>(column);

		if (id == null)
			throw new Exception($"Failed to read column: {column} as type: {typeof(TColumn)} for row reference.");

		return self.ReadRowReference<TColumn, TRow>(id, minValue);
	}

	public static TRow? ReadRowReferenceOffset<TColumn, TRow>(this RowParser self, ushort offset, int minValue = int.MinValue)
		where TRow : Lumina.Excel.ExcelRow
	{
		TColumn? id = self.ReadOffset<TColumn>(offset);

		if (id == null)
			throw new Exception($"Failed to read offset: {offset} as type: {typeof(TColumn)} for row reference.");

		return self.ReadRowReference<TColumn, TRow>(id, minValue);
	}

	private static ImageReference? CreateImageReference<TColumn>(TColumn? id)
	{
		if (id == null)
			return null;

		if (id is ushort uVal)
		{
			return new ImageReference(uVal);
		}
		else if (id is int iVal)
		{
			return new ImageReference(iVal);
		}
		else if (id is uint uiVal)
		{
			return new ImageReference(uiVal);
		}

		Logging.Shared.Error($"Unrecognised image reference column type: {typeof(TColumn)}");
		return null;
	}

	private static TRow? ReadRowReference<TColumn, TRow>(this RowParser self, TColumn id, int minValue = int.MinValue)
		where TRow : Lumina.Excel.ExcelRow
	{
		DataSheet<TRow>? sheet = GameDataService.Get<TRow>();
		if (sheet == null)
			return null;

		if (id is byte bVal)
		{
			return sheet.GetRow((byte)Math.Max(bVal, minValue));
		}
		else if (id is uint uVal)
		{
			return sheet.GetRow((uint)Math.Max(uVal, minValue));
		}
		else if (id is int iVal)
		{
			return sheet.GetRow((uint)Math.Max(iVal, minValue));
		}
		else if (id is ushort sVal)
		{
			return sheet.GetRow((ushort)Math.Max(sVal, minValue));
		}

		Logging.Shared.Error($"Unrecognized row reference key type: {typeof(TColumn)}");
		return null;
	}
}
