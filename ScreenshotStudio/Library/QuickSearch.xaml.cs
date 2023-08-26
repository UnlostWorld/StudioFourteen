// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Library;

using Lumina.Excel.GeneratedSheets;
using ScreenshotStudio.Tags;
using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using XivToolsWpf;
using XivToolsWpf.Extensions;
using XivToolsWpf.Utils;

public partial class QuickSearch : PanelWindow
{
	private static QuickSearch? instance;
	private readonly FuncQueue searchQueue;

	private Type? targetType;

	public QuickSearch()
	{
		instance = this;
		this.InitializeComponent();
		this.Tags.CollectionChanged += this.OnTagsChanged;
		this.searchQueue = new(this.SearchAsync, 250);
	}

	public string SearchTitle { get; private set; } = "Library Search";
	public TagCollection? AvailableTags { get; private set; }
	public TagCollection Tags { get; init; } = new();
	public FastObservableCollection<object> Results { get; init; } = new();

	public static void Show<T>(object placementTarget, string title, TagCollection defaultTags)
			where T : ILibraryItem
	{
		if (placementTarget is UIElement el)
		{
			Show<T>(el, title, defaultTags);
		}
	}

	public static void Show<T>(UIElement placementTarget, string title, TagCollection defaultTags)
		where T : ILibraryItem
	{
		if (instance == null)
		{
			Task.Run(async () =>
			{
				await Panel.ShowAsync<QuickSearch>();
				instance?.OnShow<T>(placementTarget, title, defaultTags);
			});
		}
		else
		{
			instance.OnShow<T>(placementTarget, title, defaultTags);
		}
	}

	public void OnShow<T>(UIElement placementTarget, string title, TagCollection defaultTags)
		where T : ILibraryItem
	{
		this.targetType = typeof(T);

		this.Tags.Replace(defaultTags);
		this.NotifyPropertyChanged(nameof(QuickSearch.Tags));

		this.SearchTitle = title;
		this.NotifyPropertyChanged(nameof(QuickSearch.SearchTitle));

		this.AvailableTags = this.Services.Library.GetAvailableTags<T>();
		this.NotifyPropertyChanged(nameof(QuickSearch.AvailableTags));
	}

	protected override void OnClosed()
	{
		base.OnClosed();
		instance = null;
	}

	private void OnTagsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		this.searchQueue.Invoke();
	}

	private async Task SearchAsync()
	{
		if (this.targetType == null)
			return;

		await this.Dispatcher.MainThread();
		TagCollection tags = new(this.Tags);

		await Dispatch.NonUiThread();

		List<ILibraryItem> results = this.Services.Library.Search(this.targetType, tags);

		await this.Dispatcher.MainThread();
		this.Results.Replace(results);
	}
}
