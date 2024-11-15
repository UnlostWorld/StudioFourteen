namespace Lumina.Excel;

public static class RowRefExtensions
{
	public static bool IsRow<T>(this RowRef<T> self, IExcelRow<T>? row)
		where T : struct, IExcelRow<T>
	{
		if (!self.IsValid)
			return row is null;

		if (row is null)
			return !self.IsValid;

		return self.RowId == row.RowId;
	}

	public static bool IsRow<T>(this RowRef<T> self, uint row)
		where T : struct, IExcelRow<T>
	{
		return self.RowId == row;
	}
}