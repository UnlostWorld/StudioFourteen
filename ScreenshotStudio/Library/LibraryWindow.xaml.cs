namespace ScreenshotStudio.Library;

using ScreenshotStudio.Tags;
using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfUtils;
using WpfUtils.Extensions;
using WpfUtils.Utils;

public partial class LibraryWindow : PanelWindow
{
	private readonly FuncQueue searchQueue;
	private string search = string.Empty;
	private TagCollection tags = new();
	private bool isLoading = false;
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

	public FastObservableCollection<object> Entries { get; init; } = new();
	public bool ViewList { get; set; } = false;

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

		this.isLoading = true;
		this.isLoading = false;

		/*List<ILibraryItem> results = this.Services.Library.Search(targetTypes, tags, query);

		this.Log.Information($">> {results.Count} results");

		await this.Dispatcher.MainThread();

		this.isLoading = true;
		this.Entries.Replace(results);
		////this.ResultsList.ScrollIntoView(this.SelectedItem);
		this.isLoading = false;*/
	}

	private void OnTabChanged(object sender, RoutedEventArgs e)
	{
		if (this.isLoading)
		{
		}
	}

	private void OnRevertClicked(object sender, RoutedEventArgs e)
	{
	}

	private void OnItemDoubleClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
	}

	private void OnFavoritesChecked(object sender, RoutedEventArgs e)
	{
	}

	private void OnDirectorySelected(object sender, RoutedEventArgs e)
	{
	}

	private void OnBackClicked(object sender, RoutedEventArgs e)
	{
	}

	private void OnApplyClicked(object sender, RoutedEventArgs e)
	{
	}
}