namespace ScreenshotStudio.Library;

using ScreenshotStudio.Library.Filters;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Tags;
using System;
using System.Collections.Generic;

/// <summary>
/// An group entry is an entry in the library that contains other entries, such as a directory or folder.
/// </summary>
public abstract class GroupEntryBase : EntryBase
{
	private readonly List<IEntryBase> allEntries = new();
	private readonly List<IEntryBase> filteredEntries = new();

	protected GroupEntryBase(SourceBase? source)
		: base(source)
	{
	}

	public IEnumerable<IEntryBase>? FilteredEntries => this.filteredEntries;
	public IEnumerable<IEntryBase>? AllEntries => this.allEntries;

	public int AllCount => this.allEntries.Count;

	public void Add(IEntryBase entry)
	{
		this.allEntries.Add(entry);
	}

	public void Clear()
	{
		this.allEntries.Clear();
		this.filteredEntries.Clear();
	}

	public override bool PassesFilters(params FilterBase[] filters)
	{
		return this.filteredEntries.Count > 0;
	}

	public void FilterEntries(params FilterBase[] filters)
	{
		if (this.allEntries.Count <= 0)
			return;

		try
		{
			this.filteredEntries.Clear();

			if (filters.Length <= 0)
			{
				this.filteredEntries.AddRange(this.allEntries);
			}
			else
			{
				foreach (IEntryBase entry in this.allEntries)
				{
					if (entry == null)
						continue;

					if (entry is GroupEntryBase dir)
					{
						dir.FilterEntries(filters);
					}

					if (entry.PassesFilters(filters))
					{
						this.filteredEntries.Add(entry);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Logging.Shared.Error(ex, "Exception while filtering entries");
		}
	}

	public IEnumerable<IEntryBase>? GetFilteredEntries(bool flatten)
	{
		if (!flatten)
			return this.FilteredEntries;

		List<IEntryBase> entries = new();
		this.Flatten(ref entries);
		return entries;
	}

	public void GetAllTags(ref TagCollection tags)
	{
		if (this.FilteredEntries == null)
			return;

		foreach (IEntryBase entry in this.FilteredEntries)
		{
			if (entry.Tags != null)
			{
				tags.AddRange(entry.Tags);
			}

			if (entry is GroupEntryBase dir)
			{
				dir.GetAllTags(ref tags);
			}
		}
	}

	public override void Dispose()
	{
		base.Dispose();

		foreach (IEntryBase entry in this.allEntries)
		{
			if (entry == null)
				continue;

			entry.Dispose();
		}
	}

	private void Flatten(ref List<IEntryBase> entries)
	{
		if (this.FilteredEntries == null)
			return;

		foreach (IEntryBase entry in this.FilteredEntries)
		{
			if (entry is GroupEntryBase dir)
			{
				dir.Flatten(ref entries);
			}
			else
			{
				entries.Add(entry);
			}
		}
	}
}
