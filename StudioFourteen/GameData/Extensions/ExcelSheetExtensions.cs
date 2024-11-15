namespace Lumina.Excel;

using System;

public static class ExcelSheetExtensions
{
	public static T GetRow<T>(this ExcelSheet<T> self, Enum v)
	where T : struct, IExcelRow<T>
	{
		return self.GetRow(Convert.ToUInt32(v));
	}
}
