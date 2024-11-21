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

namespace StudioFourteen.Library;

using FontAwesome.Sharp;
using PropertyChanged.SourceGenerator;
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
using WpfUtils.Controls;
using WpfUtils.Extensions;
using WpfUtils.Utils;

using Panel = StudioFourteen.Panels.Panel;

public partial class LibraryWindow : Panel
{
	public static LibraryTab AllTab = new("All", IconChar.List);
	public static LibraryTab FavoritesTab = new("Favorites", IconChar.Heart, new LibraryFavoritesFilter());
	public static LibraryTab AppearancesTab = new("Appearances", IconChar.UserShield);
	public static LibraryTab PosesTab = new("Poses", IconChar.PersonRunning);
	public static LibraryTab ScenesTab = new("scenes", IconChar.Users);

	private readonly FuncQueue searchQueue;
	private readonly Stopwatch searchStopwatch = new();
	private bool flatten = false;
	[Notify] private Result? selectedResult = null;
	private Navigation navigation = Navigation.None;
	[Notify] private NavigationAnimations navigationAnimation = NavigationAnimations.None;
	[Notify] private bool viewList;

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
	}

	public enum NavigationAnimations
	{
		None,
		OpenDir_Out,
		OpenDir_In,
		Back_In,
		Back_Out,
	}

	public List<LibraryTab> Tabs { get; init; } = new()
	{
		AllTab,
		FavoritesTab,
		AppearancesTab,
		PosesTab,
		ScenesTab,
	};

	public FastObservableCollection<Result> Results { get; init; } = new();
	public FastObservableCollection<GroupEntryBase> Path { get; init; } = new();
	public TagCollection AvailableTags { get; init; } = new();
	public TagFilter TagFilter { get; init; } = new();
	public SearchQueryFilter SearchQueryFilter { get; init; } = new();

	[AutoNotify] public GroupEntryBase? CurrentGroup => this.Path.Count > 0 ? this.Path[this.Path.Count - 1] : null;
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

	public LibraryTab CurrentTab
	{
		get
		{
			int index = this.GetPersistence<int>();

			if (index < 0 | index >= this.Tabs.Count)
				index = 0;

			return this.Tabs[index];
		}
		set
		{
			int oldIndex = this.GetPersistence<int>();
			int index = this.Tabs.IndexOf(value);
			this.SetPersistence(index);
			this.NotifyPropertyChanged();

			// clear the path
			this.Path.Clear();
			this.Path.Add(this.Services.Library.Root);

			this.navigation = Navigation.OpenDir;
			this.searchQueue.InvokeImmediate();
		}
	}

	public static void Open(LibraryTab? tab = null)
	{
		OpenAsync(tab).Run();
	}

	public static async Task OpenAsync(LibraryTab? tab = null)
	{
		LibraryWindow? panel = ServiceManager.Instance.Panels.Get<LibraryWindow>();
		if (panel == null)
			panel = await ServiceManager.Instance.Panels.Open<LibraryWindow>();

		if (panel == null)
			return;

		if (tab != null)
		{
			await panel.Dispatcher.InvokeAsync(() => panel.CurrentTab = tab);
		}
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

		filters.AddRange(this.CurrentTab.Filters);

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

	private void OnDirectorySwapClicked(object sender, RoutedEventArgs e)
	{
		if (sender is Button button)
		{
			GroupEntryBase? subGroup = button.DataContext as GroupEntryBase;
			if (subGroup == null)
				return;

			GroupEntryBase? baseGroup = subGroup.Parent;
			if (baseGroup == null)
				return;

			List<GroupEntryBase> newPath = new();
			foreach(GroupEntryBase entry in this.Path)
			{
				newPath.Add(entry);

				if (entry == baseGroup)
				{
					break;
				}
			}

			newPath.Add(subGroup);

			this.navigation = Navigation.Back;
			this.Path.Replace(newPath);
			this.searchQueue.InvokeImmediate();
		}
	}

	private void OnResultMouseEnter(object sender, MouseEventArgs e)
	{
		if (sender is not FrameworkElement senderElement)
			return;

		if (senderElement.DataContext is not Result result)
			return;

		this.LibraryContextMenu.Enter(result, senderElement);
	}

	private void OnResultMouseLeave(object sender, MouseEventArgs e)
	{
		if (sender is not FrameworkElement senderElement)
			return;

		if (senderElement.DataContext is not Result result)
			return;

		this.LibraryContextMenu.Leave(result);
	}

	private void OnResultMouseRight(object sender, MouseButtonEventArgs e)
	{
		this.LibraryContextMenu.Expand();
	}
}

public class LibraryTab(string name, IconChar icon, params FilterBase[] filters)
	: ViewModel
{
	public string Name { get; init; } = Resources.Find($"LOC_Library_{name}", name);
	public IconChar Icon { get; init; } = icon;
	public FilterBase[] Filters { get; init; } = filters;
}