namespace ScreenshotStudio.Library.Results;

using ScreenshotStudio.Library.Filters;
using ScreenshotStudio.Tags;
using System;
using System.Collections.Generic;
using static FFXIVClientStructs.FFXIV.Client.LayoutEngine.LayoutManager;

public class Result(IEntryBase entry)
{
	public IEntryBase Entry { get; set; } = entry;
}

public class GroupResult : Result
{
	private readonly List<Result> results = new();

	public GroupResult(GroupEntryBase group)
	: base(group)
	{
	}

	public IEnumerable<Result>? Results => this.results;
	public int Count => this.results.Count;

	public GroupEntryBase Group => (GroupEntryBase)this.Entry;

	public void Clear()
	{
		this.results.Clear();
	}

	public Result? Find(IEntryBase? entry)
	{
		if (entry == null)
			return null;

		if (this.Entry == entry)
			return this;

		foreach (Result result in this.results)
		{
			if (result.Entry == entry)
				return result;

			if (result is GroupResult groupResult)
			{
				Result? child = groupResult.Find(entry);
				if (child != null)
				{
					return child;
				}
			}
		}

		return null;
	}

	public bool FilterEntries(params FilterBase[] filters)
	{
		this.Clear();

		if (this.Group.AllCount <= 0)
			return false;

		IEnumerable<IEntryBase>? allEntries = this.Group.AllEntries;
		if (allEntries == null)
			return false;

		try
		{
			foreach (IEntryBase entry in allEntries)
			{
				if (entry == null)
					continue;

				if (entry is GroupEntryBase childGroup)
				{
					GroupResult childGroupResults = new(childGroup);
					if (childGroupResults.FilterEntries(filters))
					{
						this.results.Add(childGroupResults);
					}
				}
				else
				{
					bool passesFilters = true;
					foreach (FilterBase filter in filters)
					{
						passesFilters &= filter.Filter(entry);
						if (!passesFilters)
						{
							break;
						}
					}

					if (passesFilters)
					{
						this.results.Add(new(entry));
					}
				}
			}

			return this.results.Count > 0;
		}
		catch (Exception ex)
		{
			Logging.Shared.Error(ex, "Exception while filtering entries");
		}

		return false;
	}

	public IEnumerable<Result>? Get(bool flatten)
	{
		if (flatten)
		{
			List<Result> flattenResults = new();
			this.Flatten(ref flattenResults);
			return flattenResults;
		}
		else
		{
			return this.results;
		}
	}

	public void GetTags(ref TagCollection tags)
	{
		if (this.results == null)
			return;

		foreach (Result result in this.results)
		{
			if (result.Entry.Tags != null)
			{
				tags.AddRange(result.Entry.Tags);
			}

			if (result is GroupResult groupResult)
			{
				groupResult.GetTags(ref tags);
			}
		}
	}

	private void Flatten(ref List<Result> results)
	{
		foreach (Result result in this.results)
		{
			if (result is GroupResult groupResult)
			{
				groupResult.Flatten(ref results);
			}
			else
			{
				results.Add(result);
			}
		}
	}
}