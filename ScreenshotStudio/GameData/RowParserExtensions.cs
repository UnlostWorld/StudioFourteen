// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData;

using System;
using Lumina.Excel;
using Lumina.Text;
using ScreenshotStudio.Services;

public static class RowParserExtensions
{
	public static ImageReference? ReadImageReference<TColumn>(this RowParser self, int column)
	{
		TColumn? id = self.ReadColumn<TColumn>(column);

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

		DataSheet<TRow>? sheet = ServiceManager.Instance.Data.GetSheet<TRow>();
		if (sheet != null)
		{
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
		}

		Logging.Shared.Error($"Unrecognized row reference key type: {typeof(TColumn)}");
		return null;
	}
}
