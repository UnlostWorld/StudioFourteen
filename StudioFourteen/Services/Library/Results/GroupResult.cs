// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Services.Library.Results;

using StudioFourteen.Services.Library.Filters;
using StudioFourteen.Services.Library.Tags;
using System;
using System.Collections.Generic;

public class Result
{
	public Result(LibraryEntryBase entry)
	{
		this.Entry = entry;
		this.Icon = entry.Icon;
	}

	public LibraryEntryBase Entry { get; set; }
	public object? Icon { get; set; }

	public string? EntryId => this.Entry?.ToString();
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

	public Result? Find(LibraryEntryBase? entry)
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

		lock (this.Group)
		{
			if (this.Group.AllCount <= 0)
				return false;

			IEnumerable<LibraryEntryBase>? allEntries = this.Group.AllEntries;
			if (allEntries == null)
				return false;

			try
			{
				foreach (LibraryEntryBase entry in allEntries)
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
						}

						if (passesFilters)
						{
							Result result = new(entry);
							this.results.Add(result);
						}
					}
				}

				this.results.Sort((a, b) =>
				{
					// Favorites to top
					if (a.Entry.IsFavorite && !b.Entry.IsFavorite)
						return -1;

					if (!a.Entry.IsFavorite && b.Entry.IsFavorite)
						return 1;

					// Groups next
					if (a.Entry is GroupEntryBase && b.Entry is not GroupEntryBase)
						return -1;

					if (a.Entry is not GroupEntryBase && b.Entry is GroupEntryBase)
						return 1;

					// then everything else
					return a.Entry.DefaultSortValue.CompareTo(b.Entry.DefaultSortValue);
				});

				return this.results.Count > 0;
			}
			catch (Exception ex)
			{
				Studio.Log.Error(ex, "Exception while filtering entries");
			}
		}

		return false;
	}

	public List<Result>? Get(bool flatten)
	{
		if (flatten)
		{
			List<Result> flattenResults = new();
			this.Flatten(ref flattenResults);

			flattenResults.Sort((a, b) =>
			{
				// Favorites to top
				if (a.Entry.IsFavorite && !b.Entry.IsFavorite)
					return -1;

				if (!a.Entry.IsFavorite && b.Entry.IsFavorite)
					return 1;

				return 0;
			});

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