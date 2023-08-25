// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Library;

using Lumina.Data.Parsing.Layer;
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
	}

	public void RemoveProvider(LibraryProvider provider)
	{
		this.providers.Add(provider);
	}

	public override Task Start()
	{
		TagCollection tags = new();
		tags.Add("White Mage");
		this.Search<ITagged>(tags);

		return base.Start();
	}

	public List<T> Search<T>(TagCollection tags)
		where T : ITagged
	{
		 return this.Search<T>(typeof(T), tags);
	}

	public List<ITagged> Search(Type targetType, TagCollection tags)
	{
		return this.Search<ITagged>(targetType, tags);
	}

	private List<T> Search<T>(Type targetType, TagCollection tags)
		where T : ITagged
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

				if (!tObj.Tags.Matches(tags))
					continue;

				results.Add(tObj);
			}
		}

		sw.Stop();
		this.Log.Information($"Searched {checkCount} items and found {results.Count} items in {sw.ElapsedMilliseconds}ms");

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
	public abstract IEnumerator GetEnumerator();
	public abstract bool Contains(Type targetType);
}

public interface ILibraryItem : ITagged
{
	bool Search(TagCollection tags);
}