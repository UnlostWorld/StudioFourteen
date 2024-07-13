namespace ScreenshotStudio.GameData;

using Lumina.Excel;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.Library;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Plugin;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

public class DataSheetLibrarySource : SourceBase
{
	private readonly DataSheet sheet;
	private readonly List<IEntryBase> allEntries = new();

	public DataSheetLibrarySource(DataSheet sheet)
	{
		this.sheet = sheet;
	}

	public override string Name => Resources.Find($"LOC_Sheet{this.sheet.RowType.Name}", this.sheet.RowType.Name);

	public override void Scan()
	{
		foreach(ExcelRow row in this.sheet)
		{
			if (row is LibraryExcelRow excelRow)
			{
				excelRow.Source = this;
			}

			if (row is IEntryBase entry)
			{
				if (!entry.IsValid)
					continue;

				this.Add(entry);
			}
		}
	}

	public override string ToString() => this.GetInternalId();
	protected override string GetInternalId() => $"DataSheet_{this.sheet.RowType.Name}";
}

public abstract class DataSheet : IEnumerable
{
	protected readonly ILogger Log;
	private readonly DataSheetLibrarySource librarySource;

	public DataSheet()
	{
		this.Log = Logging.ForContext(this.GetType());
		this.librarySource = new(this);
	}

	public virtual uint Count => 0;
	public bool IsInitialized { get; private set; } = false;
	public abstract Type RowType { get; }
	protected ServiceManager Services => ServiceManager.Instance;

	public abstract IEnumerator GetEnumerator();

	public virtual Task Initialize()
	{
		this.Services.Library.AddSource(this.librarySource);
		this.IsInitialized = true;
		return Task.CompletedTask;
	}

	public virtual Task Shutdown()
	{
		return Task.CompletedTask;
	}
}

public class DataSheet<T> : DataSheet
	where T : ExcelRow
{
	public DataSheet()
		: base()
	{
		this.Sheet = DalamudServices.DataManager?.GetExcelSheet<T>();

		if (this.Sheet == null)
		{
			this.Log.Error($"Failed to get excel sheet for type: {typeof(T)}");
		}
	}

	public override uint Count => this.Sheet?.RowCount ?? 0;
	public override Type RowType => typeof(T);
	protected ExcelSheet<T>? Sheet { get; init; }

	public override Task Shutdown()
	{
		// If this is a custom sheet, get it out of the sheet cache so it unloads.
		if (typeof(T).IsAssignableTo(typeof(StudioExcelRow)))
		{
			if (this.Sheet != null)
			{
				FieldInfo? field = this.Sheet.GetType().GetField("_rowCache", BindingFlags.NonPublic | BindingFlags.Instance);
				if (field != null)
				{
					IDictionary? dict = field.GetValue(this.Sheet) as IDictionary;
					if (dict != null)
					{
						dict.Clear();
					}
				}
			}

			DalamudServices.DataManager?.Excel.RemoveSheetFromCache<T>();
		}

		return base.Shutdown();
	}

	public T? GetRow(int row) => this.Sheet?.GetRow((uint)row);
	public T? GetRow(uint row) => this.Sheet?.GetRow(row);
	public T? GetRow(uint row, uint subRow) => this.Sheet?.GetRow(row, subRow);

	public override IEnumerator GetEnumerator()
	{
		if (this.Sheet == null)
			return new List<T>().GetEnumerator();

		return this.Sheet.GetEnumerator();
	}

	public IEnumerable<T?> GetFrom(int startRow)
	{
		List<T?> results = new List<T?>();

		if (startRow >= this.Count)
			return results;

		for (int i = startRow; i < this.Count; i++)
		{
			results.Add(this.GetRow(i));
		}

		return results;
	}
}
