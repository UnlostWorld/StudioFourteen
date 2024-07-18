namespace ScreenshotStudio.Library;

using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Tags;
using System.Collections.Generic;
using System.ComponentModel;

/// <summary>
/// An group entry is an entry in the library that contains other entries, such as a directory or folder.
/// </summary>
public abstract class GroupEntryBase : EntryBase
{
	private readonly List<IEntryBase> allEntries = new();

	protected GroupEntryBase(SourceBase? source)
		: base(source)
	{
	}

	public IEnumerable<IEntryBase>? AllEntries => this.allEntries;

	public int AllCount => this.allEntries.Count;

	public void Add(IEntryBase entry)
	{
		this.allEntries.Add(entry);
	}

	public void Clear()
	{
		this.allEntries.Clear();
		this.NotifyPropertyChanged(nameof(GroupEntryBase.AllEntries));
	}

	public void GetAllTags(ref TagCollection tags)
	{
		if (this.allEntries == null)
			return;

		foreach (IEntryBase entry in this.allEntries)
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
}
