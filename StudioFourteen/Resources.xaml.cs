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

namespace StudioFourteen;

using FFXIVClientStructs.FFXIV.Common.Lua;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

public partial class Resources : ResourceDictionary
{
	private static readonly List<Uri> PendingMergedDictionaries = new();
	private static readonly Dictionary<object, Func<object>> AddResource = new();
	private static readonly List<WeakReference<Resources>> ResourceInstances = new();

	private readonly Dispatcher? ownerDispatcher;

	public Resources()
	{
		this.ownerDispatcher = Dispatcher.CurrentDispatcher;
	}

	public static Resources? Shared { get; private set; }

	public static Resources Load()
	{
		LoadShared();

		Resources resources = new();
		resources.Source = new("pack://application:,,,/StudioFourteen;component/Resources.xaml");

		foreach (Uri dictionary in PendingMergedDictionaries)
		{
			ResourceDictionary merged = new();
			merged.Source = dictionary;
			resources.MergedDictionaries.Add(merged);
		}

		foreach ((object key, Func<object> value) in AddResource)
		{
			resources[key] = value.Invoke();
		}

		lock (ResourceInstances)
		{
			ResourceInstances.Add(new(resources));
		}

		return resources;
	}

	public static void LoadShared()
	{
		if (Shared != null)
			return;

		try
		{
			Shared = new();
			Shared.Source = new("pack://application:,,,/StudioFourteen;component/Resources.xaml");

			foreach (Uri dictionary in PendingMergedDictionaries)
			{
				ResourceDictionary merged = new();
				merged.Source = dictionary;
				Shared.MergedDictionaries.Add(merged);
			}

			foreach ((object key, Func<object> value) in AddResource)
			{
				Shared[key] = value.Invoke();
			}
		}
		catch (Exception ex)
		{
			Logging.Shared.Error(ex, "Error loading shared resources");
		}
	}

	public static void UnMergeDictionary(Uri uri)
	{
		PendingMergedDictionaries.Remove(uri);

		foreach (WeakReference<Resources> resourceReference in ResourceInstances.AsReadOnly())
		{
			if (resourceReference.TryGetTarget(out Resources? resource) && resource != null)
			{
				try
				{
					resource.ownerDispatcher?.Invoke(() =>
					{
						foreach (ResourceDictionary dict in resource.MergedDictionaries)
						{
							if (dict.Source == uri)
							{
								resource.MergedDictionaries.Remove(dict);
								break;
							}
						}
					});
				}
				catch (TaskCanceledException)
				{
				}
			}
		}
	}

	public static void MergeDictionary(Uri uri)
	{
		PendingMergedDictionaries.Add(uri);

		foreach (WeakReference<Resources> resourceReference in ResourceInstances.AsReadOnly())
		{
			if (resourceReference.TryGetTarget(out Resources? resource) && resource != null)
			{
				try
				{
					resource.ownerDispatcher?.Invoke(() =>
					{
						ResourceDictionary merged = new();
						merged.Source = uri;
						resource.MergedDictionaries.Add(merged);
					});
				}
				catch (TaskCanceledException)
				{
				}
			}
		}
	}

	public static void Set(object key, Func<object> value)
	{
		AddResource[key] = value;

		foreach (WeakReference<Resources> resourceReference in ResourceInstances.AsReadOnly())
		{
			if (resourceReference.TryGetTarget(out Resources? resource) && resource != null)
			{
				resource.ownerDispatcher?.Invoke(() =>
				{
					resource[key] = value.Invoke();
				});
			}
		}
	}

	public static void Clear(object key)
	{
		AddResource.Remove(key);

		foreach (WeakReference<Resources> resourceReference in ResourceInstances.AsReadOnly())
		{
			if (resourceReference.TryGetTarget(out Resources? resource) && resource != null)
			{
				resource.ownerDispatcher?.Invoke(() =>
				{
					resource[key] = null;
				});
			}
		}
	}

	public static string Find(object key, string fallback)
	{
		object? val = Find(key, (object?)fallback);

		if (val is string valStr)
			return valStr;

		return fallback;
	}

	public static object? Find(object key, object? fallback = null)
	{
		if (Shared == null)
		{
			Logging.Shared.Warning($"No shared resources for lookup: {key}, using fallback {fallback}");
			return fallback;
		}

		if (Shared.Contains(key))
			return Shared[key];

		return fallback;
	}

	public static string Format(object key, params object?[] args)
	{
		return string.Format(Find(key, string.Empty), args);
	}
}
