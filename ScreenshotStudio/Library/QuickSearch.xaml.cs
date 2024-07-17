namespace ScreenshotStudio.Library;

using ScreenshotStudio.Library.Filters;
using ScreenshotStudio.Services;
using ScreenshotStudio.Tags;
using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfUtils;
using WpfUtils.Extensions;
using WpfUtils.Utils;

using Panel = ScreenshotStudio.Windows.Panel;

public partial class QuickSearch : DockPanel
{
	private static QuickSearch? instance;
	private readonly FuncQueue searchQueue;

	private Type? targetType;
	private object? selectedItem;
	private Action<object, bool>? selectionChanged;
	private bool isLoading = false;

	public QuickSearch()
	{
		instance = this;
		this.InitializeComponent();
		this.TagFilter.Tags.CollectionChanged += this.OnTagsChanged;
		this.searchQueue = new(this.SearchAsync, 250);

		this.Services.Input.AddListener(Input.KeyBindEvents.Interface_InvokeQuickSearch, this.OnInvoke);
	}

	[AutoNotify] public FastObservableCollection<object> Results { get; init; } = new();
	[AutoNotify] public TagFilter TagFilter { get; init; } = new();
	[AutoNotify] public SearchQueryFilter SearchQueryFilter { get; init; } = new();
	[AutoNotify] public TypeFilter? TypeFilter { get; protected set; }
	[AutoNotify] public TagCollection AvailableTags { get; init; } = new();
	[AutoNotify] public string SearchTitle { get; set; } = "Library Search";

	public string? Search
	{
		get => this.SearchQueryFilter.Search;
		set
		{
			this.SearchQueryFilter.Search = value;
			this.NotifyPropertyChanged();
			this.searchQueue.Invoke();
		}
	}

	public object? SelectedItem
	{
		get => this.selectedItem;
		set
		{
			this.selectedItem = value;

			if (value != null && !this.isLoading)
				this.selectionChanged?.Invoke(value, false);

			this.NotifyPropertyChanged();
		}
	}

	public void OnInvoke()
	{
		this.Dispatcher.Invoke(() =>
		{
			if (this.Visibility == Visibility.Collapsed)
			{
				this.Visibility = Visibility.Visible;
				this.StudioButton.IsEnabled = true;
			}
			else
			{
				this.Visibility = Visibility.Collapsed;
				this.StudioButton.IsEnabled = false;
			}
		});
	}

	public void OnShow<T>(string title, TagCollection defaultTags, T? current, Action<T, bool> selectionChanged)
		where T : IEntryBase
	{
		this.isLoading = true;
		this.targetType = typeof(T);

		this.selectionChanged = (obj, isFinal) =>
		{
			if (obj is T item)
			{
				selectionChanged.Invoke(item, isFinal);
			}
		};

		this.TagFilter.Tags.Replace(defaultTags);
		this.TypeFilter = new(typeof(T));

		this.SearchTitle = title;

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

		await Dispatch.NonUiThread();

		List<FilterBase> filters = new List<FilterBase>();
		filters.Add(this.TagFilter);
		filters.Add(this.SearchQueryFilter);

		if (this.TypeFilter != null)
			filters.Add(this.TypeFilter);

		this.Services.Library.Root.FilterEntries(filters.ToArray());
		IEnumerable<IEntryBase>? results = this.Services.Library.Root.GetFilteredEntries(true);

		await this.Dispatcher.MainThread();

		this.isLoading = true;

		TagCollection tags = new();
		this.Services.Library.Root.GetAllTags(ref tags);
		this.AvailableTags.Replace(tags);

		if (results != null)
		{
			this.Results.Replace(results);
			this.ResultsList.ScrollIntoView(this.SelectedItem);
		}

		this.isLoading = false;
	}

	private void OnConfirmClicked(object sender, RoutedEventArgs? e)
	{
		this.Close();

		if (this.selectedItem == null)
			return;

		this.selectionChanged?.Invoke(this.selectedItem, true);
	}

	private void OnResultsListKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return && this.selectedItem != null)
		{
			this.OnConfirmClicked(sender, null);
		}
	}

	private void ResultsListDoubleClicked(object sender, MouseButtonEventArgs e)
	{
		this.OnConfirmClicked(sender, null);
	}
}
