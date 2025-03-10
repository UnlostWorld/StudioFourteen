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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyChanged.SourceGenerator;
using StudioFourteen;
using StudioFourteen.Files;
using StudioFourteen.Input;
using StudioFourteen.Library.Filters;
using StudioFourteen.Library.Results;
using StudioFourteen.Library.Sources;
using StudioFourteen.Mvm;
using StudioFourteen.Tags;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Text;
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
	public static LibraryTab AllTab = new("All", IconChar.List);
	public static LibraryTab FavoritesTab = new("Favorites", IconChar.Heart, new LibraryFavoritesFilter());
	public static LibraryTab AppearancesTab = new("Appearances", IconChar.UserShield);
	public static LibraryTab PosesTab = new("Poses", IconChar.PersonRunning);
	public static LibraryTab ScenesTab = new("Scenes", IconChar.Users);

	private readonly FuncQueue searchQueue;
	private readonly FuncQueue stopPreviewQueue;
	private readonly Stopwatch searchStopwatch = new();
	private readonly LibraryDoubleClickContext resultExecutionContext = new();
	private LibraryPreviewBase? currentPreview;
	private bool flatten = false;
	[Notify] private Result? selectedResult = null;
	private Navigations navigation = Navigations.None;
	[Notify] private NavigationAnimations navigationAnimation = NavigationAnimations.None;
	[Notify] private bool viewList;
	[Notify] private bool narrowMode;
	private FrameworkElement? currentHover;
	private int lastEntryClick = 0;
	private double? waitingForPosition;

	public LibraryWindow()
	{
		this.searchQueue = new(this.SearchAsync, 250);
		this.stopPreviewQueue = new(this.StopPreview, 250);
		this.TagFilter.Tags.CollectionChanged += this.OnTagsFilterChanged;
		this.Services.Library.ScanComplete += this.OnLibraryScanComplete;

		this.SizeChanged += this.OnSizeChanged;
	}

	public enum Navigations
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

	public string? PersistentPath
	{
		get => this.GetPersistence<string>();
		set => this.SetPersistence(value);
	}

	public TagCollection? PersistentTags
	{
		get => this.GetPersistence<TagCollection>();
		set => this.SetPersistence(value);
	}

	public double? PersistentScrollPosition
	{
		get => this.GetPersistence<double>();
		set => this.SetPersistence(value);
	}

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
			this.SavePath();

			this.navigation = Navigations.OpenDir;
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
		this.LoadPath();

		if (this.PersistentTags != null)
			this.TagFilter.Tags.Replace(this.PersistentTags);

		this.waitingForPosition = this.PersistentScrollPosition;

		this.navigation = Navigations.OpenDir;
		this.searchQueue.InvokeImmediate();
	}

	protected override void OnClosed()
	{
		ScrollViewer? scroll = this.ResultsGrid.FindChild<ScrollViewer>();
		if (scroll != null)
			this.PersistentScrollPosition = scroll.VerticalOffset;

		base.OnClosed();
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
			Navigations.OpenDir => NavigationAnimations.OpenDir_Out,
			Navigations.Back => NavigationAnimations.Back_Out,
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

		bool hasFocus = this.ResultsGrid.IsKeyboardFocusWithin || this.ResultsGrid.IsKeyboardFocused;

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
			Navigations.OpenDir => NavigationAnimations.OpenDir_In,
			Navigations.Back => NavigationAnimations.Back_In,
			_ => NavigationAnimations.None,
		};

		this.navigation = Navigations.None;

		if (this.SelectedResult == null && this.Results.Count > 0)
			this.SelectedResult = this.Results[0];

		if (this.waitingForPosition != null)
		{
			ScrollViewer? scroll = this.ResultsGrid.FindChild<ScrollViewer>();
			if (scroll != null)
			{
				scroll.ScrollToVerticalOffset((double)this.waitingForPosition);
			}
		}
		else
		{
			this.ResultsGrid.ScrollIntoView(this.SelectedResult);
		}

		DependencyObject? item = this.ResultsGrid.ItemContainerGenerator.ContainerFromItem(this.SelectedResult);
		if (hasFocus && item is UIElement el)
		{
			Navigation.SetFocus(el);
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

			this.SavePath();

			this.navigation = Navigations.Back;
			this.searchQueue.InvokeImmediate();
		}
	}

	private void OnBackClicked(object sender, RoutedEventArgs e)
	{
		this.navigation = Navigations.Back;
		this.Path.RemoveAt(this.Path.Count - 1);
		this.searchQueue.InvokeImmediate();
	}

	private void OnTagsFilterChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		this.searchQueue.InvokeImmediate();
		this.PersistentTags = this.TagFilter.Tags;
	}

	private void OnInfoTagSelected(Tag tag)
	{
		this.TagFilter.Tags.Add(tag);
	}

	private async void OnBrowseClicked(object sender, RoutedEventArgs e)
	{
		FileInfo? file = await this.Services.Files.ShowOpenDialog(null, typeof(SceneFile), typeof(PoseFile), typeof(AppearanceFile));
		if (file == null)
			return;

		FileTypeInfoBase? typeInfo = this.Services.Files.GetTypeInfo(file);
		if (typeInfo == null)
			return;

		FileSource? fileSource = this.Services.Library.GetSource<FileSource>();
		if (fileSource == null)
			return;

		await this.resultExecutionContext.Execute(fileSource.Get(file, typeInfo));

		await this.MainThread();
		this.Close();
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

			this.navigation = Navigations.Back;
			this.Path.Replace(newPath);
			this.SavePath();
			this.searchQueue.InvokeImmediate();
		}
	}

	// Hijack the result tooltip logic.
	private void OnResultToolTipOpening(object sender, ToolTipEventArgs? e)
	{
		if (e != null)
			e.Handled = true;

		if (sender is not FrameworkElement senderElement)
			return;

		if (senderElement.DataContext is not Result result)
			return;

		this.currentHover = senderElement;
		this.LibraryContextMenu.Enter(result.Entry, senderElement);
		this.StartPreview().Run();
	}

	private void OnResultMouseLeft(object sender, MouseButtonEventArgs e)
	{
		int clickDelta = e.Timestamp - this.lastEntryClick;
		this.lastEntryClick = e.Timestamp;

		if (clickDelta > 500)
			return;

		if (sender is not FrameworkElement senderElement)
			return;

		if (senderElement.DataContext is not Result result)
			return;

		if (result != this.selectedResult)
			return;

		this.OnResultEnter(sender, e);
	}

	private void OnResultMouseRight(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;

		if (sender is not FrameworkElement senderElement)
			return;

		if (this.ResultsGrid.SelectedItems.Count == 1)
		{
			object? selected = this.ResultsGrid.SelectedItems[0];
			if (selected is Result result)
			{
				this.LibraryContextMenu.Enter(result.Entry, senderElement);
				this.LibraryContextMenu.Expand();
			}
		}
		else if (this.ResultsGrid.SelectedItems.Count > 1)
		{
			List<LibraryEntryBase> entries = new();
			foreach(object? obj in this.ResultsGrid.SelectedItems)
			{
				if (obj is Result result)
				{
					entries.Add(result.Entry);
				}
			}

			this.LibraryContextMenu.Enter(entries, senderElement);
			this.LibraryContextMenu.Expand();
		}
	}

	private void OnListEnter(object sender, RoutedEventArgs e)
	{
		if (this.SelectedResult == null)
			return;

		this.OnResultEnter(this.SelectedResult, e);
	}

	private void OnListBack(object sender, RoutedEventArgs e)
	{
		if (this.SelectedResult == null)
			return;

		this.OnBackClicked(sender, e);
	}

	private async void OnResultEnter(object sender, RoutedEventArgs e)
	{
		if (this.currentPreview != null)
		{
			await this.currentPreview.StopPreviewAsync();
			this.currentPreview = null;
		}

		if (this.SelectedResult is GroupResult groupResult)
		{
			this.Path.Add(groupResult.Group);
			this.SavePath();
			this.navigation = Navigations.OpenDir;
			this.searchQueue.InvokeImmediate();
		}
		else if (this.SelectedResult is Result result)
		{
			await this.resultExecutionContext.Execute(result.Entry);
		}
	}

	private void OnSizeChanged(object sender, SizeChangedEventArgs e)
	{
		this.NarrowMode = e.NewSize.Width < 450;
	}

	private void StopPreview()
	{
		this.currentPreview?.StopPreview();
	}

	private async Task StartPreview()
	{
		await this.MainThread();

		if (this.currentHover == null || this.currentHover.DataContext is not Result result)
			return;

		LibraryPreviewBase? nextPreview = result.Entry.GetPreview();
		if (nextPreview != null)
		{
			this.stopPreviewQueue.Cancel();

			LibraryPreviewBase? lastPreview = this.currentPreview;
			if (lastPreview?.HasStopped == true)
				lastPreview = null;

			this.currentPreview = nextPreview;
			this.currentPreview?.StartPreview(lastPreview);
		}
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		if (this.currentHover != null)
		{
			Point p = Mouse.GetPosition(this.currentHover);

			if (p.X >= -5
				&& p.Y >= -5
				&& p.X <= this.currentHover.ActualWidth + 5
				&& p.Y <= this.currentHover.ActualHeight + 5)
			{
				return;
			}

			if (this.currentHover.DataContext is Result result)
			{
				this.LibraryContextMenu.Leave(result.Entry);
				this.stopPreviewQueue.Invoke();
			}

			this.currentHover = null;
		}
	}

	private void SavePath()
	{
		StringBuilder sb = new();
		foreach(GroupEntryBase segment in this.Path)
		{
			if (segment == this.Services.Library.Root)
				continue;

			sb.Append(segment.Identifier);
			sb.Append("/");
		}

		this.PersistentPath = sb.ToString();
	}

	private void LoadPath()
	{
		string? path = this.PersistentPath;
		if (path == null)
			return;

		string[] segments = path.Split("/");

		List<GroupEntryBase> groups = new();
		GroupEntryBase? current = this.Services.Library.Root;
		groups.Add(current);

		foreach(string identifier in segments)
		{
			current = current?.GetGroup(identifier);
			if (current != null)
			{
				groups.Add(current);
			}
		}

		this.Path.Replace(groups);
	}
}

public class LibraryTab(string name, IconChar icon, params FilterBase[] filters)
	: ViewModel
{
	public string Name { get; init; } = Resources.Find($"LOC_Library_{name}", name);
	public IconChar Icon { get; init; } = icon;
	public FilterBase[] Filters { get; init; } = filters;
}
