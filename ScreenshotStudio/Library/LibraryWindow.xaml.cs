namespace ScreenshotStudio.Library;

using FontAwesome.Sharp;
using FontAwesome.Sharp.Pro;
using ScreenshotStudio.Library.Filters;
using ScreenshotStudio.Library.Results;
using ScreenshotStudio.Services;
using ScreenshotStudio.Tags;
using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfUtils;
using WpfUtils.Commands;
using WpfUtils.Extensions;
using WpfUtils.Utils;
using static FFXIVClientStructs.FFXIV.Client.LayoutEngine.LayoutManager;

public partial class LibraryWindow : PanelWindow
{
	private readonly FuncQueue searchQueue;
	private LibraryTab currentTab;
	private bool flatten = false;

	public LibraryWindow()
	{
		this.searchQueue = new(this.SearchAsync, 250);

		this.TagFilter.Tags.CollectionChanged += this.OnTagsFilterChanged;

		// Remember?
		this.currentTab = this.Tabs[0];
	}

	[AutoNotify]
	public FastObservableCollection<LibraryTab> Tabs { get; init; } = new()
	{
		new("LOC_Library_Favorites", ProIcons.Heart, new LibraryFavoritesFilter()),
		new("LOC_Library_Poses", ProIcons.Running, new TagFilter()),
		new("LOC_Library_Characters", ProIcons.User, new TypeFilter(typeof(ICharacterAppearance))),
		new("LOC_Library_Scenes", ProIcons.Users,  new TagFilter()),
	};

	public LibraryTab CurrentTab
	{
		get => this.currentTab;
		set
		{
			this.currentTab = value;
			this.NotifyPropertyChanged();

			// clear the path
			this.Path.Clear();
			this.Path.Add(this.Services.Library.Root);

			this.searchQueue.InvokeImmediate();
		}
	}

	[AutoNotify] public FastObservableCollection<Result> Results { get; init; } = new();
	[AutoNotify] public Result? SelectedResult { get; set; } = null;
	[AutoNotify] public bool ViewList { get; set; } = false;
	[AutoNotify] public ObservableCollection<GroupEntryBase> Path { get; init; } = new();
	[AutoNotify] public GroupEntryBase CurrentGroup => this.Path[this.Path.Count - 1];
	[AutoNotify] public TagCollection AvailableTags { get; init; } = new();
	[AutoNotify] public TagFilter TagFilter { get; init; } = new();
	[AutoNotify] public SearchQueryFilter SearchQueryFilter { get; init; } = new();
	[AutoNotify] public bool CanChangeFlatten => this.SearchQueryFilter.IsEmpty;

	public bool Flatten
	{
		get => this.flatten;
		set
		{
			this.flatten = value;
			this.NotifyPropertyChanged();
			this.searchQueue.InvokeImmediate();
		}
	}

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

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Path.Clear();
		this.Path.Add(this.Services.Library.Root);

		this.searchQueue.Invoke();
	}

	private async Task SearchAsync()
	{
		await this.Dispatcher.MainThread();

		bool flattenResults = this.flatten;
		flattenResults |= !this.SearchQueryFilter.IsEmpty;

		await Dispatch.NonUiThread();

		List<FilterBase> filters = new List<FilterBase>();
		filters.AddRange(this.CurrentTab.Filters);
		filters.Add(this.TagFilter);
		filters.Add(this.SearchQueryFilter);

		GroupResult result = new(this.CurrentGroup);
		result.FilterEntries(filters.ToArray());
		IEnumerable<Result>? results = result.Get(flattenResults);

		await this.Dispatcher.MainThread();

		if (results == null)
		{
			this.Results.Clear();
		}
		else
		{
			this.Results.Replace(results);
		}

		TagCollection tags = new();
		result.GetTags(ref tags);
		this.AvailableTags.Replace(tags);

		////this.ResultsList.ScrollIntoView(this.SelectedItem);
	}

	private void OnItemDoubleClicked(object sender, MouseButtonEventArgs e)
	{
		if (this.SelectedResult is GroupResult groupResult)
		{
			this.Path.Add(groupResult.Group);
			this.searchQueue.InvokeImmediate();
		}
		else if (this.SelectedResult is Result result)
		{
			result.Entry?.Execute();
		}
	}

	private void OnDirectorySelected(object sender, RoutedEventArgs e)
	{
		if (sender is Button btn && btn.DataContext is GroupEntryBase group)
		{
			int index = this.Path.IndexOf(group);

			while (this.Path.Count > index + 1)
			{
				this.Path.RemoveAt(index + 1);
			}

			this.searchQueue.InvokeImmediate();
		}
	}

	private void OnBackClicked(object sender, RoutedEventArgs e)
	{
		this.Path.RemoveAt(this.Path.Count - 1);
		this.searchQueue.InvokeImmediate();
	}

	private void OnTagsFilterChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		this.searchQueue.Invoke();
	}

	private void OnInfoTagSelected(Tag tag)
	{
		this.TagFilter.Tags.Add(tag);
	}
}

public class LibraryTab(string name, ProIcons icon, params FilterBase[] filters)
	: ViewModel
{
	public string Name { get; init; } = Resources.Find(name, string.Empty);
	public ProIcons Icon { get; init; } = icon;
	public FilterBase[] Filters { get; init; } = filters;
}