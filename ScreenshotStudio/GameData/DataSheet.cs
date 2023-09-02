// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData;

using Lumina.Excel;
using ScreenshotStudio.Library;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Tags;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract class DataSheet : LibraryProvider
{
	protected readonly ILogger Log;

	public DataSheet()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	public bool IsInitialized { get; private set; } = false;
	public abstract Type RowType { get; }
	protected ServiceManager Services => ServiceManager.Instance;

	public virtual Task Initialize()
	{
		this.Services.Library.AddProvider(this);
		this.IsInitialized = true;
		return Task.CompletedTask;
	}

	public virtual Task Shutdown()
	{
		this.Services.Library.RemoveProvider(this);
		return Task.CompletedTask;
	}
}

public class DataSheet<T> : DataSheet
	where T : ExcelRow
{
	public DataSheet()
		: base()
	{
		this.Sheet = DalamudServices.DataManager.GetExcelSheet<T>();

		if (this.Sheet == null)
		{
			this.Log.Error($"Failed to get excel sheet for type: {typeof(T)}");
		}
	}

	public virtual uint Count => this.Sheet?.RowCount ?? 0;
	public override Type RowType => typeof(T);
	protected ExcelSheet<T>? Sheet { get; init; }

	public T? GetRow(int row) => this.Sheet?.GetRow((uint)row);
	public T? GetRow(uint row) => this.Sheet?.GetRow(row);
	public T? GetRow(uint row, uint subRow) => this.Sheet?.GetRow(row, subRow);

	public override IEnumerator GetEnumerator()
	{
		if (this.Sheet == null)
			return new List<T>().GetEnumerator();

		return this.Sheet.GetEnumerator();
	}

	public override bool Contains(Type targetType) => targetType.IsAssignableFrom(typeof(T));

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

	protected override void GetAllTags(ref TagCollection tags)
	{
		foreach (T item in this)
		{
			if (item is ITagged taggedItem)
			{
				tags.Add(taggedItem.Tags);
			}
		}
	}
}
