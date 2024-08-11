namespace ScreenshotStudio.Library;

using ScreenshotStudio.Library.Executors;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Tags;
using System.Collections.Generic;
using System.ComponentModel;

/// <summary>
/// An group entry is an entry in the library that contains other entries, such as a directory or folder.
/// </summary>
public abstract class GroupEntryBase : LibraryEntryBase
{
	private readonly List<ILibraryEntry> allEntries = new();

	protected GroupEntryBase(SourceBase? source)
		: base(source)
	{
	}

	public IEnumerable<ILibraryEntry>? AllEntries => this.allEntries;

	public int AllCount => this.allEntries.Count;

	public virtual void Add(ILibraryEntry entry)
	{
		lock (this)
		{
			this.allEntries.Add(entry);
		}
	}

	public virtual void Clear()
	{
		lock (this)
		{
			this.allEntries.Clear();
			this.NotifyPropertyChanged(nameof(GroupEntryBase.AllEntries));
		}
	}

	public void GetAllTags(ref TagCollection tags)
	{
		if (this.allEntries == null)
			return;

		foreach (ILibraryEntry entry in this.allEntries)
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

		foreach (ILibraryEntry entry in this.allEntries)
		{
			if (entry == null)
				continue;

			entry.Dispose();
		}
	}

	public sealed override EntryExecutor? GetExecutor()
	{
		return null;
	}
}
