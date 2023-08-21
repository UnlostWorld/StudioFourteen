// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData;
using Lumina.Excel;
using ScreenshotStudio.Plugin;
using Serilog;
using System.Collections;
using System.Collections.Generic;

public class DataSheet<T> : IEnumerable<T>
	where T : ExcelRow
{
	public DataSheet()
	{
		this.Log = Serilog.Log.ForContext(this.GetType());
		this.Sheet = DalamudServices.DataManager.GetExcelSheet<T>();

		if (this.Sheet == null)
		{
			this.Log.Error($"Failed to get excel sheet for type: {typeof(T)}");
		}
	}

	protected ILogger Log { get; init; }

	protected ExcelSheet<T>? Sheet { get; init; }

	public T? GetRow(uint row) => this.Sheet?.GetRow(row);
	public T? GetRow(uint row, uint subRow) => this.Sheet?.GetRow(row, subRow);

	public IEnumerator<T> GetEnumerator()
	{
		if (this.Sheet == null)
			return new List<T>().GetEnumerator();

		return this.Sheet.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (this.Sheet == null)
			return new List<T>().GetEnumerator();

		return this.Sheet.GetEnumerator();
	}
}
