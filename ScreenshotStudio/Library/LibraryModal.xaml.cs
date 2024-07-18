namespace ScreenshotStudio.Library;

using ScreenshotStudio.Library.Filters;
using ScreenshotStudio.Library.Results;
using ScreenshotStudio.Services;
using ScreenshotStudio.Tags;
using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfUtils;
using WpfUtils.Extensions;
using WpfUtils.Utils;

using Panel = ScreenshotStudio.Windows.Panel;

public partial class LibraryModal : PanelWindow
{
	private static LibraryModal? instance;
	private readonly FuncQueue searchQueue;

	private Type? targetType;
	private IEntryBase? currentEntry;
	private Result? selectedResult;
	private Action<object, bool>? selectionChanged;
	private bool isLoading = false;

	public LibraryModal()
	{
		instance = this;
		this.InitializeComponent();
		this.TagFilter.Tags.CollectionChanged += this.OnTagsChanged;
		this.searchQueue = new(this.SearchAsync, 250);
	}

	[AutoNotify] public FastObservableCollection<Result> Results { get; init; } = new();
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

	public Result? SelectedResult
	{
		get => this.selectedResult;
		set
		{
			this.selectedResult = value;
			this.currentEntry = value?.Entry;

			if (value != null && !this.isLoading)
				this.selectionChanged?.Invoke(value.Entry, false);

			this.NotifyPropertyChanged();
		}
	}

	public static void Show<T>(object placementTarget, string title, TagCollection defaultTags, T? current, Action<T, bool> selectionChanged)
			where T : IEntryBase
	{
		if (placementTarget is UIElement el)
		{
			Show<T>(el, title, defaultTags, current, selectionChanged);
		}
	}

	public static void Show<T>(UIElement placementTarget, string title, TagCollection defaultTags, T? current, Action<T, bool> selectionChanged)
		where T : IEntryBase
	{
		if (instance == null)
		{
			Task.Run(async () =>
			{
				await Panel.ShowAsync<LibraryModal>();
				instance?.OnShow<T>(placementTarget, title, defaultTags, current, selectionChanged);
			});
		}
		else
		{
			instance.OnShow<T>(placementTarget, title, defaultTags, current, selectionChanged);
		}
	}

	public void OnShow<T>(UIElement placementTarget, string title, TagCollection defaultTags, T? current, Action<T, bool> selectionChanged)
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

		this.currentEntry = current;
		this.isLoading = false;

		/*Point pos;
		placementTarget.Dispatcher.Invoke(() =>
		{
			PanelWindow? targetPanel = placementTarget.FindParent<PanelWindow>();

			if (targetPanel != null)
			{
				////Point targetOffset = placementTarget.TransformToAncestor(targetPanel).Transform(new());
				pos = new Point(targetPanel.Position.X, targetPanel.Position.Y);
			}
		});

		this.Dispatcher.Invoke(() =>
		{
			this.Position = pos;
		});*/
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

		GroupResult result = new(this.Services.Library.Root);
		result.FilterEntries(filters.ToArray());
		IEnumerable<Result>? results = result.Get(true);

		Result? selectedResult = result.Find(this.currentEntry);

		await this.Dispatcher.MainThread();

		this.isLoading = true;

		TagCollection tags = new();
		result.GetTags(ref tags);
		this.AvailableTags.Replace(tags);

		if (results != null)
		{
			this.Results.Replace(results);
			this.SelectedResult = selectedResult;
			this.ResultsList.ScrollIntoView(this.SelectedResult);
		}

		this.isLoading = false;
	}

	private void OnConfirmClicked(object sender, RoutedEventArgs? e)
	{
		this.Close();

		if (this.selectedResult == null)
			return;

		this.selectionChanged?.Invoke(this.selectedResult.Entry, true);
	}

	private void OnResultsListKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return && this.selectedResult != null)
		{
			this.OnConfirmClicked(sender, null);
		}
	}

	private void ResultsListDoubleClicked(object sender, MouseButtonEventArgs e)
	{
		this.OnConfirmClicked(sender, null);
	}
}
