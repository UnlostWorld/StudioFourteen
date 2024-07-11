namespace ScreenshotStudio.Library;

using FontAwesome.Sharp;
using FontAwesome.Sharp.Pro;
using ScreenshotStudio.Library.Filters;
using ScreenshotStudio.Services;
using ScreenshotStudio.Tags;
using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfUtils;
using WpfUtils.Extensions;
using WpfUtils.Utils;

public partial class LibraryWindow : PanelWindow
{
	private readonly FuncQueue searchQueue;
	private string search = string.Empty;
	private TagCollection tags = new();
	private LibraryTab currentTab;

	public LibraryWindow()
	{
		this.searchQueue = new(this.SearchAsync, 250);

		// Remember?
		this.currentTab = this.Tabs[0];
	}

	[AutoNotify]
	public FastObservableCollection<LibraryTab> Tabs { get; init; } = new()
	{
		new("Favorites", ProIcons.Heart, new LibraryFavoritesFilter()),
		new("Poses", ProIcons.Running, new TagFilter()),
		new("Characters", ProIcons.User, new TypeFilter(typeof(IActorAppearance))),
		new("Scenes", ProIcons.Users,  new TagFilter()),
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

	[AutoNotify] public FastObservableCollection<object> Entries { get; init; } = new();
	[AutoNotify] public EntryBase? SelectedEntry { get; set; } = null;
	[AutoNotify] public bool ViewList { get; set; } = false;
	[AutoNotify] public ObservableCollection<GroupEntryBase> Path { get; init; } = new();
	[AutoNotify] public GroupEntryBase CurrentGroup => this.Path[this.Path.Count - 1];

	public TagCollection Tags
	{
		get => this.tags;
		set
		{
			this.tags = value;
			this.NotifyPropertyChanged();
		}
	}

	public string Search
	{
		get => this.search;
		set
		{
			this.search = value;
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
		TagCollection tags = new(this.Tags);
		string[] query = SearchUtility.ToQuery(this.Search);

		await Dispatch.NonUiThread();

		this.CurrentGroup.FilterEntries(this.CurrentTab.Filters);
		IEnumerable<IEntryBase>? results = this.CurrentGroup.GetFilteredEntries(false);

		await this.Dispatcher.MainThread();

		if (results == null)
		{
			this.Entries.Clear();
		}
		else
		{
			this.Entries.Replace(results);
		}

		////this.ResultsList.ScrollIntoView(this.SelectedItem);
	}

	private void OnRevertClicked(object sender, RoutedEventArgs e)
	{
	}

	private void OnItemDoubleClicked(object sender, MouseButtonEventArgs e)
	{
		if (this.SelectedEntry is GroupEntryBase group)
		{
			this.Path.Add(group);
			this.searchQueue.InvokeImmediate();
		}
	}

	private void OnFavoritesChecked(object sender, RoutedEventArgs e)
	{
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

	private void OnApplyClicked(object sender, RoutedEventArgs e)
	{
	}
}

public class LibraryTab(string name, ProIcons icon, params FilterBase[] filters)
	: ViewModel
{
	public string Name { get; private set; } = name;
	public ProIcons Icon { get; private set; } = icon;
	public FilterBase[] Filters { get; private set; } = filters;
}