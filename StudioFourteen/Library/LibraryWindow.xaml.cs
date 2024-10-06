namespace StudioFourteen.Library;

using FontAwesome.Sharp;
using StudioFourteen;
using StudioFourteen.Appearance;
using StudioFourteen.Files;
using StudioFourteen.Library.Filters;
using StudioFourteen.Library.Results;
using StudioFourteen.Mvm;
using StudioFourteen.Services;
using StudioFourteen.Tags;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfUtils;
using WpfUtils.Extensions;
using WpfUtils.Utils;

using Panel = StudioFourteen.Panels.Panel;

public partial class LibraryWindow : Panel
{
	private readonly FuncQueue searchQueue;
	private readonly Stopwatch searchStopwatch = new();
	private bool flatten = false;
	private Result? selectedResult = null;
	private Navigation navigation = Navigation.None;

	public LibraryWindow()
	{
		this.searchQueue = new(this.SearchAsync, 250);

		this.TagFilter.Tags.CollectionChanged += this.OnTagsFilterChanged;
		this.Services.Library.ScanComplete += this.OnLibraryScanComplete;
	}

	public enum Navigation
	{
		None,
		OpenDir,
		Back,
		TabLeft,
		TabRight,
	}

	public enum NavigationAnimations
	{
		None,
		OpenDir_Out,
		OpenDir_In,
		Back_In,
		Back_Out,
	}

	[AutoNotify] public NavigationAnimations NavigationAnimation { get; set; } = NavigationAnimations.None;
	[AutoNotify] public FastObservableCollection<Result> Results { get; init; } = new();
	[AutoNotify] public bool ViewList { get; set; } = false;
	[AutoNotify] public ObservableCollection<GroupEntryBase> Path { get; init; } = new();
	[AutoNotify] public GroupEntryBase? CurrentGroup => this.Path.Count > 0 ? this.Path[this.Path.Count - 1] : null;
	[AutoNotify] public TagCollection AvailableTags { get; init; } = new();
	[AutoNotify] public TagFilter TagFilter { get; init; } = new();
	[AutoNotify] public SearchQueryFilter SearchQueryFilter { get; init; } = new();
	[AutoNotify] public bool CanChangeFlatten => this.SearchQueryFilter.IsEmpty;
	[AutoNotify] public bool IsLiveExecute { get; set; }

	[AutoNotify] public Result? SelectedResult
	{
		get => this.selectedResult;
		set => this.selectedResult = value;
	}

	[AutoNotify]
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

	[AutoNotify]
	public bool Favorites
	{
		get => this.GetPersistence<bool>();
		set
		{
			this.SetPersistence(value);

			// clear the path
			this.Path.Clear();
			this.Path.Add(this.Services.Library.Root);
			this.searchQueue.InvokeImmediate();
		}
	}

	public static void Open()
	{
		OpenAsync().Run();
	}

	public static async Task OpenAsync()
	{
		LibraryWindow? panel = ServiceManager.Instance.Panels.Get<LibraryWindow>();
		if (panel == null)
			panel = await ServiceManager.Instance.Panels.Open<LibraryWindow>();

		if (panel == null)
			return;

		////await panel.Dispatcher.InvokeAsync(() => panel.CurrentTab = panel.Tabs[(int)tab]);
	}

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Path.Clear();
		this.Path.Add(this.Services.Library.Root);

		this.searchQueue.Invoke();
	}

	private void OnLibraryScanComplete()
	{
		this.searchQueue.InvokeImmediate();
	}

	private async Task SearchAsync()
	{
		if (this.CurrentGroup == null)
			return;

		this.searchStopwatch.Restart();
		await this.Dispatcher.MainThread();

		this.NavigationAnimation = this.navigation switch
		{
			Navigation.OpenDir => NavigationAnimations.OpenDir_Out,
			Navigation.Back => NavigationAnimations.Back_Out,
			_ => NavigationAnimations.None,
		};

		bool flattenResults = this.flatten;
		flattenResults |= !this.SearchQueryFilter.IsEmpty;

		await Dispatch.NonUiThread();

		List<FilterBase> filters = new List<FilterBase>();

		if (this.Favorites)
			filters.Add(new LibraryFavoritesFilter());

		filters.Add(this.TagFilter);
		filters.Add(this.SearchQueryFilter);

		GroupResult result = new(this.CurrentGroup);
		result.FilterEntries(filters.ToArray());
		IEnumerable<Result>? results = result.Get(flattenResults);

		await this.Dispatcher.MainThread();

		while (this.NavigationAnimation != NavigationAnimations.None)
			await Task.Delay(10);

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

		await Task.Delay(33);

		this.NavigationAnimation = this.navigation switch
		{
			Navigation.OpenDir => NavigationAnimations.OpenDir_In,
			Navigation.Back => NavigationAnimations.Back_In,
			_ => NavigationAnimations.None,
		};

		this.navigation = Navigation.None;

		////this.ResultsList.ScrollIntoView(this.SelectedItem);
	}

	private void OnItemDoubleClicked(object sender, MouseButtonEventArgs e)
	{
		if (this.SelectedResult is GroupResult groupResult)
		{
			this.Path.Add(groupResult.Group);
			this.navigation = Navigation.OpenDir;
			this.searchQueue.InvokeImmediate();
		}
		else if (this.SelectedResult is Result result && result.Entry is ILibraryActions actions)
		{
			actions.Apply(this.Services.Target.TargetObjectIndex).Run();
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

			this.navigation = Navigation.Back;
			this.searchQueue.InvokeImmediate();
		}
	}

	private void OnBackClicked(object sender, RoutedEventArgs e)
	{
		this.navigation = Navigation.Back;
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

	private void OnBrowseClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Files.ShowOpenDialog(null, typeof(SceneFile), typeof(PoseFile), typeof(AppearanceFile));
	}
}

public class LibraryTab(string name, IconChar icon, params FilterBase[] filters)
	: ViewModel
{
	public string Name { get; init; } = Resources.Find(name, string.Empty);
	public IconChar Icon { get; init; } = icon;
	public FilterBase[] Filters { get; init; } = filters;
}