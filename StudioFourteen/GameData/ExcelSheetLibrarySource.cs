namespace StudioFourteen.GameData;

using Lumina.Excel;
using StudioFourteen.GameData.Library;
using StudioFourteen.Library.Sources;
using System;

public abstract class ExcelSheetLibrarySource : SourceBase
{
}

public class ExcelSheetLibrarySource<TExcelType, TEntryType> : ExcelSheetLibrarySource
	where TExcelType : struct, IExcelRow<TExcelType>
	where TEntryType : ExcelLibraryEntry
{
	public readonly ExcelSheet<TExcelType>? Sheet;
	public readonly Type RowType = typeof(TExcelType);

	public ExcelSheetLibrarySource()
	{
		this.Sheet = this.Services.GameData.GetSheet<TExcelType>();
	}

	public override string Name => Resources.Find($"LOC_Sheet{this.RowType.Name}", this.RowType.Name);
	public override string ToString() => this.GetInternalId();
	protected override string GetInternalId() => $"ExcelSheet_{this.RowType.Name}";

	protected override void Scan()
	{
		if (this.Sheet == null)
			return;

		foreach(TExcelType row in this.Sheet)
		{
			TEntryType? entry = (TEntryType?)Activator.CreateInstance(typeof(TEntryType), [this, row]);
			if (entry == null)
				continue;

			this.Add(entry);
		}
	}
}