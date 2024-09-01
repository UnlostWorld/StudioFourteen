namespace ScreenshotStudio.Library;

using FFXIVClientStructs;
using ScreenshotStudio.GameData.Excel;
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

public partial class LibraryModal : Panel
{
	private static LibraryModal? instance;
	private readonly FuncQueue searchQueue;

	private object? currentEntry;
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
		where T : notnull
	{
		if (placementTarget is UIElement el)
		{
			Show(el, title, defaultTags, current, (s, f) => selectionChanged.Invoke((T)s, f));
		}
	}

	public static void Show(UIElement placementTarget, string title, TagCollection defaultTags, Type type, object? current, Action<object, bool> selectionChanged)
	{
		if (instance == null)
		{
			Task.Run(async () =>
			{
				instance = await ServiceManager.Instance.Panels.Open<LibraryModal>();
				instance?.OnShow(placementTarget, title, defaultTags, type, current, selectionChanged);
			});
		}
		else
		{
			instance.OnShow(placementTarget, title, defaultTags, type, current, selectionChanged);
		}
	}

	protected override void OnClosed()
	{
		base.OnClosed();
		instance = null;
	}

	private void OnShow(UIElement placementTarget, string title, TagCollection defaultTags, Type type, object? current, Action<object, bool> selectionChanged)
	{
		this.isLoading = true;

		this.selectionChanged = (obj, isFinal) =>
		{
			selectionChanged.Invoke(obj, isFinal);
		};

		this.TagFilter.Tags.Replace(defaultTags);
		this.TypeFilter = new(type);

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

	private void OnTagsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		this.searchQueue.Invoke();
	}

	private async Task SearchAsync()
	{
		await Dispatch.NonUiThread();

		List<FilterBase> filters = new List<FilterBase>();
		filters.Add(this.TagFilter);
		filters.Add(this.SearchQueryFilter);

		if (this.TypeFilter != null)
			filters.Add(this.TypeFilter);

		GroupResult result = new(this.Services.Library.Root);
		result.FilterEntries(filters.ToArray());
		List<Result>? results = result.Get(true);

		results?.Sort((a, b) =>
		{
			// TODO: a generic sorting system...
			// TODO: Favorites
			if (a.Entry is CharacterBackupAppearance && b.Entry is not CharacterBackupAppearance)
			{
				return 1;
			}
			else if (a.Entry is not CharacterBackupAppearance && b.Entry is CharacterAppearanceService)
			{
				return -1;
			}
			else if (a.Entry is LibraryExcelRow aRow && b.Entry is LibraryExcelRow bRow)
			{
				return aRow.RowId.CompareTo(bRow.RowId);
			}

			return a.Entry.Name?.CompareTo(b.Entry.Name) ?? 0;
		});

		Result? selectedResult = result.Find(this.currentEntry as ILibraryEntry);

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
