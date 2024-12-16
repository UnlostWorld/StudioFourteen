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

namespace StudioFourteen.Library.Selector;
using DependencyPropertyGenerator;
using Serilog;
using StudioFourteen.Library.Filters;
using StudioFourteen.Library.Results;
using StudioFourteen.Mvm;
using StudioFourteen.Tags;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfUtils;
using WpfUtils.Controls;
using WpfUtils.Utils;

[DependencyProperty<LibraryEntryBase>("SelectedItem", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<object>("Footer")]
[DependencyProperty<object>("FooterTemplate")]
[DependencyProperty<object>("BackgroundDetail")]
[DependencyProperty<TagCollection>("Tags", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<TagCollection>("AvailableTags", DefaultBindingMode = DefaultBindingMode.TwoWay, DefaultValueExpression ="new StudioFourteen.Tags.TagCollection()")]
[DependencyProperty<string>("Search", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<List<Type>>("Types", DefaultValueExpression ="new System.Collections.Generic.List<System.Type>()")]
[DependencyProperty<bool>("IsLoading", DefaultBindingMode =DefaultBindingMode.OneWay)]
[DependencyProperty<bool>("Favorites")]
public partial class LibrarySelector : PopOut
{
	protected readonly ILogger Log = Logging.ForContext<LibrarySelector>();
	private readonly FuncQueue searchQueue;
	private TextBox? searchBox;
	private ListBox? resultsBox;
	private Button? closeButton;
	private Window? targetWindow;
	private int lastEntryClick = 0;
	private Result? lastSelectedResult;

	public LibrarySelector()
	{
		this.searchQueue = new(this.SearchAsync, 250);
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		if (this.resultsBox != null)
		{
			this.resultsBox.SelectionChanged -= this.OnSelectionChanged;
			this.resultsBox.PreviewMouseUp -= this.OnResultsPreviewMouseUp;
		}

		if (this.closeButton != null)
			this.closeButton.Click -= this.OnCloseClicked;

		if (this.targetWindow != null)
			this.targetWindow.PreviewMouseDown -= this.OnTargetWindowPreviewMouseDown;

		this.searchBox = this.GetTemplateChild("PART_SearchBox") as TextBox;
		this.resultsBox = this.GetTemplateChild("PART_ResultsBox") as ListBox;
		this.closeButton = this.GetTemplateChild("PART_CloseButton") as Button;
		this.targetWindow = this.PlacementTarget.FindParent<Window>();

		if (this.resultsBox != null)
		{
			this.resultsBox.SelectionChanged += this.OnSelectionChanged;
			this.resultsBox.PreviewMouseUp += this.OnResultsPreviewMouseUp;
		}

		if (this.closeButton != null)
			this.closeButton.Click += this.OnCloseClicked;

		if (this.targetWindow != null)
			this.targetWindow.PreviewMouseDown += this.OnTargetWindowPreviewMouseDown;
	}

	protected override void OnOpened(EventArgs e)
	{
		base.OnOpened(e);

		this.searchBox?.Focus();
		this.searchQueue.InvokeImmediate();
	}

	private async Task SearchAsync()
	{
		await this.Dispatcher.MainThread();

		if (this.resultsBox == null)
			return;

		this.IsLoading = true;
		object? currentSelection = this.SelectedItem;

		List<FilterBase> filters = new List<FilterBase>();

		if (this.Favorites)
			filters.Add(new LibraryFavoritesFilter());

		if (this.Types != null && this.Types.Count > 0)
			filters.Add(new TypeFilter(this.Types));

		if (this.Tags != null)
			filters.Add(new TagFilter(this.Tags));

		if (!string.IsNullOrEmpty(this.Search))
			filters.Add(new SearchQueryFilter(this.Search));

		await Dispatch.NonUiThread();

		GroupResult result = new(ServiceManager.Instance.Library.Root);
		result.FilterEntries(filters.ToArray());
		List<Result>? results = result.Get(true);

		Result? selectedResult = result.Find(currentSelection as LibraryEntryBase);

		await this.Dispatcher.MainThread();

		TagCollection tags = new();
		result.GetTags(ref tags);
		this.AvailableTags?.Replace(tags);

		this.resultsBox.ItemsSource = null;
		this.resultsBox.ItemsSource = results;
		this.resultsBox.SelectedItem = selectedResult;
		this.resultsBox.ScrollIntoView(selectedResult);

		this.IsLoading = false;
	}

	partial void OnSearchChanged()
	{
		this.searchQueue.Invoke();
	}

	partial void OnFavoritesChanged()
	{
		this.searchQueue.Invoke();
	}

	partial void OnTagsChanged(TagCollection? oldValue, TagCollection? newValue)
	{
		if (oldValue != null)
		{
			oldValue.CollectionChanged -= this.OnTagsCollectionChanged;
		}

		if (newValue != null)
		{
			newValue.CollectionChanged += this.OnTagsCollectionChanged;
		}

		this.searchQueue.Invoke();
	}

	private void OnTagsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		this.searchQueue.Invoke();
	}

	private void OnCloseClicked(object sender, RoutedEventArgs e)
	{
		this.IsOpen = false;
	}

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (this.resultsBox == null)
			return;

		Result? selectedResult = this.resultsBox.SelectedItem as Result;
		this.SelectedItem = selectedResult?.Entry;
	}

	private void OnTargetWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (!this.IsOpen)
			return;

		if (this.IsMouseOver)
			return;

		e.Handled = true;
		this.IsOpen = false;
	}

	private void OnResultsPreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		int clickDelta = e.Timestamp - this.lastEntryClick;
		this.lastEntryClick = e.Timestamp;

		Result? currentResult = this.resultsBox?.SelectedItem as Result;
		if (this.lastSelectedResult == currentResult)
		{
			if (clickDelta < 500)
			{
				this.IsOpen = false;
			}
		}

		this.lastSelectedResult = currentResult;
	}
}
