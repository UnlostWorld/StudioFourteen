namespace ScreenshotStudio.Tags;

using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using System.Windows.Input;
using WpfUtils;
using WpfUtils.Extensions;
using WpfUtils.Utils;

public partial class TagSelector : UserControl, IComparer<Tag>, INotifyPropertyChanged
{
	public static readonly DependencyProperty AllTagsProperty = DependencyProperty.Register(
		nameof(TagSelector.AllTags),
		typeof(TagCollection),
		typeof(TagSelector),
		new(new TagCollection()));

	public static readonly DependencyProperty TagsProperty = DependencyProperty.Register(
		nameof(TagSelector.Tags),
		typeof(TagCollection),
		typeof(TagSelector),
		new(new TagCollection(), OnTagsChanged));

	public static readonly DependencyProperty SearchProperty = DependencyProperty.Register(
		nameof(TagSelector.Search),
		typeof(string),
		typeof(TagSelector),
		new(null));

	protected readonly ILogger Log = Logging.Shared.ForContext<TagSelector>();

	private const int MaxSuggestTags = 30;
	private readonly FuncQueue tagSearchQueue;
	private bool isChangingTags = false;
	private bool isSuggestTags;
	private bool bypassTagSearch = false;

	public TagSelector()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.tagSearchQueue = new(this.SearchAsync, 250);

		this.Loaded += this.OnLoaded;
		this.IsEnabledChanged += this.OnIsEnabledChanged;
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	public event RoutedEventHandler? OnDone;

	public bool IsSuggestTags
	{
		get => this.isSuggestTags;
		set
		{
			if (!value)
				this.bypassTagSearch = false;

			this.isSuggestTags = value;
			this.NotifyPropertyChanged(nameof(TagSelector.IsSuggestTags));
		}
	}

	public FastObservableCollection<Tag> SuggestTags { get; init; } = new();
	public Tag? SelectedSuggestTag { get; set; } = null;
	public FastObservableCollection<Tag> SelectedTags { get; init; } = new();

	public string Search
	{
		get => (string)this.GetValue(SearchProperty);
		set => this.SetValue(SearchProperty, value);
	}

	public TagCollection AllTags
	{
		get => (TagCollection)this.GetValue(AllTagsProperty);
		set => this.SetValue(AllTagsProperty, value);
	}

	public TagCollection Tags
	{
		get => (TagCollection)this.GetValue(TagsProperty);
		set => this.SetValue(TagsProperty, value);
	}

	public int Compare(Tag? x, Tag? y) => string.Compare(x?.Name, y?.Name);

	public async void SetFocus()
	{
		this.SearchTextBox.SetFocusToWindow();

		await Task.Delay(1);
		await this.Dispatcher.MainThread();

		bool result = this.SearchTextBox.Focus();
		this.SearchTextBox.CaretIndex = int.MaxValue;
	}

	protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}

	private static void OnTagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is TagSelector tagFilter)
		{
			if (tagFilter.Tags == null)
				return;

			if (e.OldValue != null && e.OldValue is TagCollection oldTc)
				oldTc.CollectionChanged -= tagFilter.OnTagsChanged;

			if (e.NewValue != null && e.NewValue is TagCollection newTc)
				newTc.CollectionChanged += tagFilter.OnTagsChanged;

			tagFilter.OnTagsChanged(null, null);
		}
	}

	private void OnTagsChanged(object? sender, NotifyCollectionChangedEventArgs? e)
	{
		if (this.isChangingTags)
			return;

		this.Dispatcher.Invoke(() =>
		{
			this.SelectedTags.Replace(this.Tags);
		});
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (this.SuggestTags != null)
		{
			this.SuggestTags.SortAndReplace(this.SuggestTags, this);
			this.IsSuggestTags = this.SuggestTags.Count > 0;
		}
	}

	private void RemoveTag(Tag tag)
	{
		this.isChangingTags = true;

		this.SelectedTags.Remove(tag);

		if (!string.IsNullOrEmpty(this.Search))
		{
			this.SuggestTags.Add(tag);
			this.SuggestTags.Sort(this);
		}

		this.Tags.Replace(this.SelectedTags);
		this.IsSuggestTags = this.SuggestTags.Count > 0;

		this.isChangingTags = false;
	}

	private void AddTag(Tag tag)
	{
		this.isChangingTags = true;

		this.SelectedTags.Add(tag);
		this.SuggestTags.Remove(tag);
		this.SuggestTags.Sort(this);
		this.Tags.Replace(this.SelectedTags);

		if (!this.bypassTagSearch)
			this.Search = string.Empty;

		this.IsSuggestTags = false;

		this.isChangingTags = false;
	}

	private void OnTagMouseDown(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
	}

	private void OnTagMouseUp(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
		if (sender is FrameworkElement el && el.DataContext is Tag tag)
		{
			ListBox? lb = el.FindParent<ListBox>();
			if (lb == null)
				return;

			if (lb.ItemsSource == this.SuggestTags)
			{
				this.AddTag(tag);
			}
			else
			{
				this.RemoveTag(tag);
			}
		}
	}

	private void OnRemoveTagMouseUp(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
		if (sender is FrameworkElement el && el.DataContext is Tag tag)
		{
			this.RemoveTag(tag);
		}
	}

	private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (this.IsEnabled)
		{
			this.SetFocus();
		}
	}

	private void OnTagSearchGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		if (!string.IsNullOrEmpty(this.Search))
		{
			this.tagSearchQueue.Invoke();
		}
	}

	private void OnTagSearchLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		this.SuggestTags.Clear();
		this.IsSuggestTags = false;

		this.OnDone?.Invoke(this, new());
	}

	private void OnTagSearchPreviewKeyDown(object sender, KeyEventArgs e)
	{
		try
		{
			if (e.Key == Key.Escape || e.Key == Key.Return)
			{
				this.SetFocusToWindow();
				e.Handled = true;

				this.OnDone?.Invoke(this, new());
			}
			else if (e.Key == Key.Tab)
			{
				if(this.SelectedSuggestTag != null)
					this.AddTag(this.SelectedSuggestTag);

				e.Handled = true;
			}
			else if (e.Key == Key.Down && this.IsSuggestTags)
			{
				this.IncrementSuggestIndex(1);
				e.Handled = true;
			}
			else if (e.Key == Key.Down && !this.IsSuggestTags)
			{
				this.bypassTagSearch = true;
				this.tagSearchQueue.InvokeImmediate();
				e.Handled = true;
			}
			else if (e.Key == Key.Up && this.IsSuggestTags)
			{
				this.IncrementSuggestIndex(-1);
				e.Handled = true;
			}
			else if (e.Key == Key.Back)
			{
				if (string.IsNullOrEmpty(this.Search) && this.SelectedTags.Count > 0)
				{
					Tag tag = this.SelectedTags[this.SelectedTags.Count - 1];
					this.RemoveTag(tag);
					this.Search = tag.Name;
					this.SearchTextBox.CaretIndex = int.MaxValue;
					e.Handled = true;
				}

				this.tagSearchQueue.Invoke();
			}
			else
			{
				this.tagSearchQueue.Invoke();
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in OnTagSearchPreviewKeyDown");
		}
	}

	private void OnShowTagsClicked(object sender, RoutedEventArgs e)
	{
		this.bypassTagSearch = true;
		this.tagSearchQueue.InvokeImmediate();
	}

	private void IncrementSuggestIndex(int amount)
	{
		int currentIndex = 0;

		if (this.SelectedSuggestTag != null)
		{
			currentIndex = this.SuggestTags.IndexOf(this.SelectedSuggestTag);

			if (currentIndex < 0)
			{
				this.SelectedSuggestTag = this.SuggestTags[0];
				this.NotifyPropertyChanged(nameof(TagSelector.SelectedSuggestTag));
				return;
			}
		}

		currentIndex += amount;

		if (currentIndex < 0)
			currentIndex = this.SuggestTags.Count - 1;

		if (currentIndex >= this.SuggestTags.Count)
			currentIndex = 0;

		this.SelectedSuggestTag = this.SuggestTags[currentIndex];
		this.NotifyPropertyChanged(nameof(TagSelector.SelectedSuggestTag));
	}

	private async Task SearchAsync()
	{
		await this.Dispatcher.MainThread();

		List<Tag> tags;
		if (!this.bypassTagSearch)
		{
			string? str = this.Search;

			HashSet<Tag> filterByTags = new(this.SelectedTags);
			HashSet<Tag>? allTags = this.AllTags != null ? new(this.AllTags) : null;

			await Dispatch.NonUiThread();

			string[]? query = null;
			if (!string.IsNullOrEmpty(str))
			{
				if (str != null)
				{
					str = str.ToLower();
					query = str.Split(' ');
				}
			}

			tags = new();
			if (allTags != null)
			{
				foreach (Tag tag in allTags)
				{
					if (filterByTags.Contains(tag))
						continue;

					if (query != null && !tag.Search(query))
						continue;

					tags.Add(tag);
				}
			}
		}
		else
		{
			tags = new(this.AllTags);
		}

		tags.Sort(this);

		await this.Dispatcher.MainThread();
		this.SuggestTags.Replace(tags);

		if (this.SuggestTags.Count > 0)
		{
			this.SelectedSuggestTag = this.SuggestTags[0];
			this.NotifyPropertyChanged(nameof(TagSelector.SelectedSuggestTag));
		}

		this.IsSuggestTags = this.SuggestTags.Count > 0;
	}
}