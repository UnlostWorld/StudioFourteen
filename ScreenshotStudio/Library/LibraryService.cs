namespace ScreenshotStudio.Library;

using ScreenshotStudio.Services;
using ScreenshotStudio.Tags;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public class LibraryService : ServiceBase
{
	private readonly List<LibraryProvider> providers = new();

	public void AddProvider(LibraryProvider provider)
	{
		this.providers.Add(provider);
		provider.CacheAllTags();
	}

	public void RemoveProvider(LibraryProvider provider)
	{
		this.providers.Add(provider);
	}

	public override Task Shutdown()
	{
		Tag.ClearTagCache();
		return base.Shutdown();
	}

	public TagCollection GetAvailableTags<T>()
		where T : ILibraryItem
	{
		TagCollection tags = new();

		foreach (LibraryProvider provider in this.providers)
		{
			if (provider.Contains(typeof(T)))
			{
				tags.Add(provider.AllTags);
			}
		}

		return tags;
	}

	public List<T> Search<T>(TagCollection tags, string[]? query)
		where T : ILibraryItem
	{
		 return this.Search<T>(typeof(T), tags, query);
	}

	public List<ILibraryItem> Search(Type targetType, TagCollection tags, string[]? query)
	{
		return this.Search<ILibraryItem>(targetType, tags, query);
	}

	private List<T> Search<T>(Type targetType, TagCollection tags, string[]? query)
		where T : ILibraryItem
	{
		Stopwatch sw = new();
		sw.Start();
		int checkCount = 0;

		List<T> results = new();
		foreach (LibraryProvider provider in this.providers)
		{
			if (!this.IsAlive)
				return results;

			if (!provider.Contains(targetType))
				continue;

			foreach (object? obj in provider)
			{
				if (!this.IsAlive)
					return results;

				checkCount++;

				if (obj is not T tObj)
					continue;

				if (!tObj.Search(tags, query))
					continue;

				results.Add(tObj);
			}
		}

		sw.Stop();
		this.Log.Information($"Searched {checkCount} {targetType.Name}s and found {results.Count} in {sw.ElapsedMilliseconds}ms");

		return results;
	}
}

public abstract class LibraryProvider<T> : LibraryProvider
	where T : ITagged
{
	public override bool Contains(Type targetType) => targetType.IsAssignableFrom(typeof(T));
}

public abstract class LibraryProvider : IEnumerable
{
	private TagCollection allTags = new();

	public TagCollection AllTags => this.allTags;

	public void CacheAllTags()
	{
		this.allTags.Clear();
		this.GetAllTags(ref this.allTags);
	}

	public abstract IEnumerator GetEnumerator();
	public abstract bool Contains(Type targetType);
	protected abstract void GetAllTags(ref TagCollection tags);
}

public interface ILibraryItem : ITagged
{
	bool Search(TagCollection tags, string[]? query);
}