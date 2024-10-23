namespace StudioFourteen.Library;

using FFXIVClientStructs;
using StudioFourteen.Appearance;
using StudioFourteen.GameData.Excel;
using StudioFourteen.Library.Filters;
using StudioFourteen.Library.Results;
using StudioFourteen.Mvm;
using StudioFourteen.Tags;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfUtils;
using WpfUtils.Controls;
using WpfUtils.Extensions;
using WpfUtils.Utils;

public partial class MiniLibraryPopOut : View
{
	private static MiniLibraryPopOut? instance;
	private static bool isInstanceOpen = false;
	private readonly FuncQueue searchQueue;

	private object? currentEntry;
	private Result? selectedResult;
	private Action<object, bool>? selectionChanged;
	private bool isLoading = false;
	private PopOut? host;

	public MiniLibraryPopOut()
	{
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

	public static void Close()
	{
		CloseAsync().Run();
	}

	public static async Task<bool> CloseAsync()
	{
		if (instance != null && instance.host != null && instance.host.IsOpen)
		{
			bool wasOpen = await instance.host.Dispatcher.InvokeAsync<bool>(() =>
			{
				if (instance.host.IsOpen)
				{
					instance.host.IsOpen = false;
					return true;
				}

				return false;
			});

			if (wasOpen)
			{
				// wait for the panel to close.
				await Task.Delay(250);
			}

			return wasOpen;
		}

		return false;
	}

	public static void Show<T>(UIElement placementTarget, string title, TagCollection defaultTags, T? current, Action<T, bool> selectionChanged)
		where T : notnull
	{
		ShowAsync<T>(placementTarget, title, defaultTags, current, selectionChanged).Run();
	}

	public static void Show(UIElement placementTarget, string title, TagCollection defaultTags, Type type, object? current, Action<object, bool> selectionChanged)
	{
		ShowAsync(placementTarget, title, defaultTags, type, current, selectionChanged).Run();
	}

	public static async Task<MiniLibraryPopOut> ShowAsync<T>(UIElement placementTarget, string title, TagCollection defaultTags, T? current, Action<T, bool> selectionChanged)
		where T : notnull
	{
		return await ShowAsync(placementTarget, title, defaultTags, typeof(T), current, (s, f) => selectionChanged.Invoke((T)s, f));
	}

	public static async Task<MiniLibraryPopOut> ShowAsync(UIElement placementTarget, string title, TagCollection defaultTags, Type type, object? current, Action<object, bool> selectionChanged)
	{
		await CloseAsync();

		if (instance == null || instance.host == null)
		{
			instance = new MiniLibraryPopOut();
			instance.host = PopOut.Show(placementTarget, instance);
			instance.host.Background = StudioFourteen.Resources.Find("ControlBackgroundBrush") as Brush;
			instance.host.StaysOpen = true;

			instance.host.Closed += (s, e) =>
			{
				isInstanceOpen = false;
			};

			isInstanceOpen = true;
		}
		else
		{
			instance.host.PlacementTarget = placementTarget;
			instance.host.IsOpen = true;
			isInstanceOpen = true;
		}

		Window? targetWindow = placementTarget.FindParent<Window>();
		if (targetWindow != null)
		{
			targetWindow.PreviewMouseDown += (s, e) =>
			{
				if (isInstanceOpen)
				{
					e.Handled = true;
					Close();
				}
			};
		}

		instance.OnShow(title, defaultTags, type, current, selectionChanged);
		return instance;
	}

	private void OnShow(string title, TagCollection defaultTags, Type type, object? current, Action<object, bool> selectionChanged)
	{
		this.isLoading = true;

		this.selectionChanged = (obj, isFinal) =>
		{
			selectionChanged.Invoke(obj, isFinal);
		};

		this.TagFilter.Tags.Replace(defaultTags);
		this.TypeFilter = new(type);
		this.SearchTitle = title;

		this.searchQueue.InvokeImmediate();

		this.currentEntry = current;
		this.isLoading = false;

		this.TagSelector.SetFocus();
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
		if (this.host != null)
			this.host.IsOpen = false;

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
		Task.Run(async () =>
		{
			await Task.Delay(100);

			this.Dispatcher.Invoke(() =>
			{
				this.OnConfirmClicked(sender, null);
			});
		});
	}
}
