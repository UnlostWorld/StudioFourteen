namespace StudioFourteen.GameData;

using Lumina.Excel;
using StudioFourteen.GameData.Excel;
using StudioFourteen.Library.Sources;
using System;

public abstract class ExcelSheetLibrarySource : SourceBase
{
}

public class ExcelSheetLibrarySource<T> : ExcelSheetLibrarySource
	where T : LibraryExcelRow
{
	public readonly ExcelSheet<T>? Sheet;
	public readonly Type RowType = typeof(T);

	public ExcelSheetLibrarySource()
	{
		this.Sheet = this.Services.GameData.GetSheet<T>();
	}

	public override string Name => Resources.Find($"LOC_Sheet{this.RowType.Name}", this.RowType.Name);
	public override string ToString() => this.GetInternalId();
	protected override string GetInternalId() => $"ExcelSheet_{this.RowType.Name}";

	protected override void Scan()
	{
		if (this.Sheet == null)
			return;

		foreach(T entry in this.Sheet)
		{
			this.Add(entry);
		}
	}
}