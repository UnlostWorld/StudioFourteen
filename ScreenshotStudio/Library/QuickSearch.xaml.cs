namespace ScreenshotStudio.Library;

using ScreenshotStudio.Library.Filters;
using ScreenshotStudio.Library.Results;
using ScreenshotStudio.Services;
using ScreenshotStudio.Tags;
using ScreenshotStudio.Windows;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfUtils;
using WpfUtils.Extensions;
using WpfUtils.Utils;

public partial class QuickSearch : DockPanel
{
	private static QuickSearch? instance;
	private readonly FuncQueue searchQueue;

	public QuickSearch()
	{
		instance = this;
		this.InitializeComponent();
		this.TagFilter.Tags.CollectionChanged += this.OnTagsChanged;
		this.searchQueue = new(this.SearchAsync, 250);

		this.Services.Input.AddListener(Input.KeyBindEvents.Interface_InvokeQuickSearch, this.OnOpenQuickSearch);
	}

	[AutoNotify] public FastObservableCollection<Result> Results { get; init; } = new();
	[AutoNotify] public TagFilter TagFilter { get; init; } = new();
	[AutoNotify] public SearchQueryFilter SearchQueryFilter { get; init; } = new();
	[AutoNotify] public TagCollection AvailableTags { get; init; } = new();

	[AutoNotify] public bool IsQuickSearchOpen { get; set; }
	[AutoNotify] public Result? SelectedResult { get; set; }

	[AutoNotify]
	public string? Search
	{
		get => this.SearchQueryFilter.Search;
		set
		{
			this.SearchQueryFilter.Search = value;
			this.searchQueue.Invoke();
		}
	}

	public void OnOpenQuickSearch()
	{
		this.Dispatcher.Invoke(() =>
		{
			this.Activate();

			this.SearchQueryFilter.Clear();
			this.TagFilter.Clear();

			TagCollection tags = new();
			this.Services.Library.Root.GetAllTags(ref tags);
			this.AvailableTags.Replace(tags);

			this.IsQuickSearchOpen = true;
		});
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
		if (this.SearchQueryFilter.IsEmpty && this.TagFilter.IsEmpty)
		{
			await this.Dispatcher.MainThread();

			this.Results.Clear();
			this.SelectedResult = null;
		}
		else
		{
			await Dispatch.NonUiThread();

			List<FilterBase> filters = new List<FilterBase>();

			if (!this.TagFilter.IsEmpty)
				filters.Add(this.TagFilter);

			if (!this.SearchQueryFilter.IsEmpty)
				filters.Add(this.SearchQueryFilter);

			GroupResult result = new(this.Services.Library.Root);
			result.FilterEntries(filters.ToArray());
			List<Result>? results = result.Get(true);

			if (results != null)
			{
				results.Sort((a, b) => a.FilterMatch.CompareTo(b.FilterMatch));

				await this.Dispatcher.MainThread();

				this.Results.Replace(results);
				this.SelectedResult = this.Results.Count > 1 ? this.Results[0] : null;
			}
		}
	}

	private void OnResultsListKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return && this.SelectedResult != null)
		{
			// RUN!
		}
	}

	private void ResultsListDoubleClicked(object sender, MouseButtonEventArgs e)
	{
		// RUN!
	}

	private void OnSearchDone(object sender, RoutedEventArgs e)
	{
		this.IsQuickSearchOpen = false;
		this.Results.Clear();
	}
}
