// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Library;

using ScreenshotStudio.Tags;
using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;
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
	private object? selectedItem;
	private string searchTitle = "Library Search";
	private TagCollection? availableTags;
	private TagCollection tags = new();
	private Action<object>? selectionChanged;
	private bool isLoading = false;

	public QuickSearch()
	{
		instance = this;
		this.InitializeComponent();
		this.Tags.CollectionChanged += this.OnTagsChanged;
		this.searchQueue = new(this.SearchAsync, 250);
	}

	public FastObservableCollection<object> Results { get; init; } = new();

	public TagCollection? AvailableTags
	{
		get => this.availableTags;
		set
		{
			this.availableTags = value;
			this.NotifyPropertyChanged();
		}
	}

	public TagCollection Tags
	{
		get => this.tags;
		set
		{
			this.tags = value;
			this.NotifyPropertyChanged();
		}
	}

	public string SearchTitle
	{
		get => this.searchTitle;
		set
		{
			this.searchTitle = value;
			this.NotifyPropertyChanged();
		}
	}

	public object? SelectedItem
	{
		get => this.selectedItem;
		set
		{
			this.selectedItem = value;

			if (value != null && !this.isLoading)
				this.selectionChanged?.Invoke(value);

			this.NotifyPropertyChanged();
		}
	}

	public static void Show<T>(object placementTarget, string title, TagCollection defaultTags, T? current, Action<T> selectionChanged)
			where T : ILibraryItem
	{
		if (placementTarget is UIElement el)
		{
			Show<T>(el, title, defaultTags, current, selectionChanged);
		}
	}

	public static void Show<T>(UIElement placementTarget, string title, TagCollection defaultTags, T? current, Action<T> selectionChanged)
		where T : ILibraryItem
	{
		if (instance == null)
		{
			Task.Run(async () =>
			{
				await Panel.ShowAsync<QuickSearch>();
				instance?.OnShow<T>(placementTarget, title, defaultTags, current, selectionChanged);
			});
		}
		else
		{
			instance.OnShow<T>(placementTarget, title, defaultTags, current, selectionChanged);
		}
	}

	public void OnShow<T>(UIElement placementTarget, string title, TagCollection defaultTags, T? current, Action<T> selectionChanged)
		where T : ILibraryItem
	{
		this.isLoading = true;
		this.targetType = typeof(T);

		this.selectionChanged = (obj) =>
		{
			if (obj is T item)
			{
				selectionChanged.Invoke(item);
			}
		};

		this.Tags.Replace(defaultTags);
		this.NotifyPropertyChanged(nameof(QuickSearch.Tags));

		this.SearchTitle = title;

		this.AvailableTags = this.Services.Library.GetAvailableTags<T>();
		this.NotifyPropertyChanged(nameof(QuickSearch.AvailableTags));

		this.SelectedItem = current;
		this.isLoading = false;
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

		this.isLoading = true;
		this.Results.Replace(results);
		this.ResultsList.ScrollIntoView(this.SelectedItem);
		this.isLoading = false;
	}
}
