// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Tags;

using System.Collections;
using System.Collections.Generic;

public class TagCollection : IEnumerable<Tag>
{
	public static readonly TagCollection Empty = new();

	private readonly HashSet<Tag> tags = new();
	private readonly List<string> searchTagStrings = new();

	public TagCollection()
	{
	}

	public TagCollection(TagCollection other)
	{
		this.AddRange(other);
	}

	public int Count => this.tags.Count;
	public string[] Query => this.searchTagStrings.ToArray();

	public void Add(string name)
	{
		this.tags.Add(new(name));
	}

	public void Add(Tag tag)
	{
		this.tags.Add(tag);

		if (tag is SearchTag search)
		{
			this.searchTagStrings.Add(search.Query);
		}
	}

	public void AddRange(IEnumerable<string> names)
	{
		foreach (string name in names)
		{
			this.Add(name);
		}
	}

	public void AddRange(IEnumerable<Tag> tags)
	{
		foreach (Tag tag in tags)
		{
			this.Add(tag);
		}
	}

	public void Replace(IEnumerable<Tag> tags)
	{
		this.tags.Clear();
		this.searchTagStrings.Clear();

		this.AddRange(tags);
	}

	public bool Matches(TagCollection other)
	{
		foreach(Tag tag in other)
		{
			if (tag is SearchTag)
				continue;

			if (!this.tags.Contains(tag))
			{
				return false;
			}
		}

		return true;
	}

	public IEnumerator<Tag> GetEnumerator() => this.tags.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => this.tags.GetEnumerator();
}