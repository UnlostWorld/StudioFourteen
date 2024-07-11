namespace ScreenshotStudio.Library;

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
	private Tabs currentTab = Tabs.Favorites;

	public LibraryWindow()
	{
		this.searchQueue = new(this.SearchAsync, 250);
	}

	public enum Tabs
	{
		Favorites,
		Poses,
		Characters,
		Scenes,
	}

	[AutoNotify] public FastObservableCollection<object> Entries { get; init; } = new();
	[AutoNotify] public EntryBase? SelectedEntry { get; set; } = null;
	[AutoNotify] public bool ViewList { get; set; } = false;
	[AutoNotify] public ObservableCollection<GroupEntryBase> Path { get; init; } = new();
	[AutoNotify] public GroupEntryBase CurrentGroup => this.Path[this.Path.Count - 1];

	public Tabs CurrentTab
	{
		get => this.currentTab;
		set
		{
			this.currentTab = value;
			this.NotifyPropertyChanged();
			this.searchQueue.Invoke();
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
		Type[] targetTypes = this.currentTab switch
		{
			Tabs.Poses => new[] { typeof(IActorAppearance) },
			Tabs.Favorites => new[] { typeof(IActorAppearance) },
			Tabs.Characters => new[] { typeof(IActorAppearance) },
			Tabs.Scenes => new[] { typeof(IActorAppearance) },
			_ => throw new Exception("No Tab"),
		};

		await this.Dispatcher.MainThread();
		TagCollection tags = new(this.Tags);
		string[] query = SearchUtility.ToQuery(this.Search);

		await Dispatch.NonUiThread();

		FilterBase[] filters = new[]
		{
			new TypeFilter("Characters", new[] { typeof(IActorAppearance) }),
		};

		this.CurrentGroup.FilterEntries(filters);
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

	private void OnTabChanged(object sender, RoutedEventArgs e)
	{
		////this.Path.RemoveRange(1, this.Path.Count - 1);
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