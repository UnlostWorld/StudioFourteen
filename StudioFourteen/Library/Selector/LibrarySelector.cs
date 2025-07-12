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
using StudioFourteen.Library.LibraryMenu;
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
[DependencyProperty<TagCollection>("Tags")]
[DependencyProperty<TagCollection>("CurrentTags", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<TagCollection>("AvailableTags", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<string>("Search", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<Type>("Type")]
[DependencyProperty<List<Type>>("Types")]
[DependencyProperty<bool>("IsLoading", DefaultBindingMode = DefaultBindingMode.OneWay)]
[DependencyProperty<bool>("CloseOnSelection")]
public partial class LibrarySelector : PopOut
{
	protected readonly ILogger Log = Logging.ForContext<LibrarySelector>();
	private readonly FuncQueue searchQueue;
	private TextBox? searchBox;
	private ListBox? resultsBox;
	private Button? closeButton;
	private LibraryContextMenu? contextMenu;
	private Window? targetWindow;
	private int lastEntryClick = 0;
	private Result? lastSelectedResult;
	private FrameworkElement? currentHover;
	private GroupResult? currentResults;

	public LibrarySelector()
	{
		this.searchQueue = new(this.SearchAsync, 250);

		this.CurrentTags = new TagCollection();
		this.AvailableTags = new TagCollection();
		this.Types = new List<Type>();

		this.OnCurrentTagsChanged(null, this.CurrentTags);
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
		{
			this.targetWindow.PreviewMouseDown -= this.OnTargetWindowPreviewMouseDown;
			this.targetWindow.MouseMove -= this.OnMouseMove;
		}

		this.searchBox = this.GetTemplateChild("PART_SearchBox") as TextBox;
		this.resultsBox = this.GetTemplateChild("PART_ResultsBox") as ListBox;
		this.closeButton = this.GetTemplateChild("PART_CloseButton") as Button;
		this.contextMenu = this.GetTemplateChild("PART_ContextMenu") as LibraryContextMenu;
		this.targetWindow = this.PlacementTarget.FindParent<Window>();

		if (this.resultsBox != null)
		{
			this.resultsBox.SelectionChanged += this.OnSelectionChanged;
			this.resultsBox.PreviewMouseUp += this.OnResultsPreviewMouseUp;
			this.resultsBox.PreviewMouseDown += this.OnResultsBoxPreviewMouseDown;
			this.resultsBox.MouseMove += this.OnMouseMove;
		}

		if (this.closeButton != null)
			this.closeButton.Click += this.OnCloseClicked;

		if (this.targetWindow != null)
		{
			this.targetWindow.PreviewMouseDown += this.OnTargetWindowPreviewMouseDown;
			this.targetWindow.MouseMove += this.OnMouseMove;
		}
	}

	// Hijack the result tooltip logic.
	public void OnResultToolTipOpening(object sender, ToolTipEventArgs? e)
	{
		if (e != null)
			e.Handled = true;

		if (sender is not FrameworkElement senderElement)
			return;

		if (senderElement.DataContext is not Result result)
			return;

		this.currentHover = senderElement;
		this.contextMenu?.Enter(result.Entry, senderElement);
	}

	public void OnResultMouseRightButtonUp(object sender, MouseButtonEventArgs e)
	{
		this.OnResultToolTipOpening(sender, null);
		this.contextMenu?.Expand();
	}

	protected override void OnOpened(EventArgs e)
	{
		base.OnOpened(e);

		if (this.Tags != null)
			this.CurrentTags?.Replace(this.Tags);

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

		if (this.Types != null && this.Types.Count > 0)
			filters.Add(new TypeFilter(this.Types));

		if (this.Type != null)
			filters.Add(new TypeFilter(this.Type));

		if (this.CurrentTags != null)
			filters.Add(new TagFilter(this.CurrentTags));

		if (!string.IsNullOrEmpty(this.Search))
			filters.Add(new SearchQueryFilter(this.Search));

		await Dispatch.NonUiThread();

		this.currentResults = new(ServiceManager.Instance.Library.Root);
		this.currentResults.FilterEntries(filters.ToArray());
		List<Result>? results = this.currentResults.Get(true);

		Result? selectedResult = this.currentResults.Find(currentSelection as LibraryEntryBase);

		await this.Dispatcher.MainThread();

		TagCollection tags = new();
		this.currentResults.GetTags(ref tags);
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

	partial void OnCurrentTagsChanged(TagCollection? oldValue, TagCollection? newValue)
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

	partial void OnSelectedItemChanged(LibraryEntryBase? oldValue, LibraryEntryBase? newValue)
	{
		if (this.resultsBox == null)
			return;

		Result? selectedResult = this.currentResults?.Find(newValue as LibraryEntryBase);
		this.resultsBox.SelectedItem = selectedResult;
	}

	partial void OnTypeChanged(Type? oldValue, Type? newValue)
	{
		this.searchQueue.Invoke();
	}

	partial void OnTypesChanged(List<Type>? oldValue, List<Type>? newValue)
	{
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

		if (this.CloseOnSelection)
		{
			this.IsOpen = false;
		}
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

	private void OnResultsBoxPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
	}

	private void OnResultsPreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			int clickDelta = e.Timestamp - this.lastEntryClick;
			this.lastEntryClick = e.Timestamp;

			Result? currentResult = this.resultsBox?.SelectedItem as Result;
			if (this.lastSelectedResult == currentResult)
			{
				if (clickDelta < 300)
				{
					this.IsOpen = false;
				}
			}

			this.lastSelectedResult = currentResult;
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
				this.contextMenu?.Leave(result.Entry);
			}

			this.currentHover = null;
		}
	}
}
