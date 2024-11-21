// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.GameData.Library;

using Lumina.Excel;
using StudioFourteen.Library.Sources;
using System;
using System.Collections.Generic;

public abstract class ExcelSheetLibrarySource : SourceBase
{
	public abstract object? GetRowObject(uint rowId);
}

public abstract class ExcelSheetLibrarySource<TEntry> : ExcelSheetLibrarySource
{
	public abstract TEntry? GetRow(uint rowId);
	public sealed override object? GetRowObject(uint rowId) => this.GetRow(rowId);
}

public class ExcelSheetLibrarySource<TExcel, TEntry> : ExcelSheetLibrarySource<TEntry>
	where TExcel : struct, IExcelRow<TExcel>
	where TEntry : ExcelLibraryEntry
{
	public readonly ExcelSheet<TExcel>? Sheet;
	public readonly Type RowType = typeof(TExcel);
	private readonly Dictionary<uint, TEntry> entryCache = new();

	public ExcelSheetLibrarySource()
	{
		this.Sheet = this.Services.GameData.GetSheet<TExcel>();
	}

	public override string Name => Resources.Find($"LOC_Sheet{this.RowType.Name}", this.RowType.Name);

	public override TEntry? GetRow(uint rowId)
	{
		TEntry? entry;
		if (this.entryCache.TryGetValue(rowId, out entry))
			return entry;

		if (this.Sheet == null)
			return null;

		if (!this.Sheet.HasRow(rowId))
			return null;

		TExcel row = this.Sheet.GetRow(rowId);
		return this.GetRow(row);
	}

	public TEntry? GetRow(TExcel row)
	{
		TEntry? entry;

		lock (this.entryCache)
		{
			if (this.entryCache.TryGetValue(row.RowId, out entry))
				return entry;

			entry = (TEntry?)Activator.CreateInstance(typeof(TEntry), [this, row]);
			if (entry == null)
				return null;

			this.entryCache.Add(row.RowId, entry);
		}

		return entry;
	}

	public override string ToString() => this.GetInternalId();
	protected override string GetInternalId() => $"ExcelSheet_{this.RowType.Name}";

	protected override void Scan()
	{
		if (this.Sheet == null)
			return;

		foreach (TExcel row in this.Sheet)
		{
			TEntry? entry = this.GetRow(row.RowId);

			if (entry == null)
				continue;

			this.Add(entry);
		}
	}
}