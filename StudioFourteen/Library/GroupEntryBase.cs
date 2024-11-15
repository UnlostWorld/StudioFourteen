namespace StudioFourteen.Library;

using FontAwesome.Sharp;
using StudioFourteen.Library.Sources;
using StudioFourteen.Tags;
using System.Collections.Generic;

/// <summary>
/// An group entry is an entry in the library that contains other entries, such as a directory or folder.
/// </summary>
public abstract class GroupEntryBase : LibraryEntryBase
{
	private readonly List<LibraryEntryBase> allEntries = new();
	private readonly List<GroupEntryBase> groupEntries = new();

	protected GroupEntryBase(SourceBase? source)
		: base(source)
	{
	}

	public virtual IconChar Icon => IconChar.None;

	public IEnumerable<LibraryEntryBase>? AllEntries => this.allEntries;
	public int AllCount => this.allEntries.Count;

	public IEnumerable<GroupEntryBase>? GroupEntries => this.groupEntries;
	public int GroupCount => this.groupEntries.Count;

	public override string? SubTitle => $"{this.AllCount} items";

	public GroupEntryBase? Parent { get; private set; }

	public bool HasSubGroups => this.GroupCount > 0;

	public virtual void Add(LibraryEntryBase entry)
	{
		lock (this)
		{
			this.allEntries.Add(entry);

			if (entry is GroupEntryBase group)
			{
				group.Parent = this;
				this.groupEntries.Add(group);
			}
		}
	}

	public virtual void Clear()
	{
		lock (this)
		{
			this.groupEntries.Clear();
			this.allEntries.Clear();

			this.NotifyPropertyChanged(nameof(GroupEntryBase.GroupEntries));
			this.NotifyPropertyChanged(nameof(GroupEntryBase.AllEntries));
		}
	}

	public void GetAllTags(ref TagCollection tags)
	{
		if (this.allEntries == null)
			return;

		foreach (LibraryEntryBase entry in this.allEntries)
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

		foreach (LibraryEntryBase entry in this.allEntries)
		{
			if (entry == null)
				continue;

			entry.Dispose();
		}
	}
}
