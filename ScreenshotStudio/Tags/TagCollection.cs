// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Tags;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

public class TagCollection : IEnumerable<Tag>, INotifyCollectionChanged
{
	public static readonly TagCollection Empty = new();

	private readonly HashSet<Tag> tags = new();

	private bool supressChangedEvents = false;

	public TagCollection()
	{
	}

	public TagCollection(TagCollection other)
		: this()
	{
		this.AddRange(other);
	}

	public event NotifyCollectionChangedEventHandler? CollectionChanged;

	public int Count => this.tags.Count;

	public Tag Add(string name)
	{
		Tag tag = Tag.Get(name);
		this.Add(tag);
		return tag;
	}

	public void Add(Tag tag)
	{
		this.tags.Add(tag);

		if (!this.supressChangedEvents)
		{
			this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, tag));
		}
	}

	public void Add(TagCollection tags)
	{
		foreach (Tag tag in tags)
		{
			this.Add(tag);
		}

		if (!this.supressChangedEvents)
		{
			this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, tags));
		}
	}

	public void Remove(Tag tag)
	{
		this.tags.Remove(tag);

		if (!this.supressChangedEvents)
		{
			this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, tag));
		}
	}

	public void AddRange(IEnumerable<string> names)
	{
		this.supressChangedEvents = true;
		foreach (string name in names)
		{
			this.Add(name);
		}

		this.supressChangedEvents = false;
	}

	public void AddRange(IEnumerable<Tag> tags)
	{
		this.supressChangedEvents = true;
		foreach (Tag tag in tags)
		{
			this.Add(tag);
		}

		this.supressChangedEvents = false;
		this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, tags));
	}

	public void Replace(IEnumerable<Tag> tags)
	{
		this.supressChangedEvents = true;

		this.Clear();
		this.AddRange(tags);

		this.supressChangedEvents = false;
		this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, tags));
	}

	public void Clear()
	{
		this.tags.Clear();

		if (!this.supressChangedEvents)
		{
			this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
		}
	}

	public bool Matches(TagCollection other)
	{
		foreach(Tag tag in other)
		{
			if (!this.tags.Contains(tag))
			{
				return false;
			}
		}

		return true;
	}

	public IEnumerator<Tag> GetEnumerator() => this.tags.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => this.tags.GetEnumerator();

	public override string ToString()
	{
		StringBuilder builder = new();
		foreach(Tag tag in this)
		{
			builder.Append(tag.Name);
			builder.Append(" ");
		}

		return builder.ToString();
	}
}