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
using System.Windows.Input;
using WpfUtils;
using WpfUtils.Extensions;
using WpfUtils.Utils;

public partial class TagFilter : UserControl, IComparer<Tag>, INotifyPropertyChanged
{
	public static readonly DependencyProperty AllTagsProperty = DependencyProperty.Register(
		nameof(TagFilter.AllTags),
		typeof(TagCollection),
		typeof(TagFilter),
		new(new TagCollection()));

	public static readonly DependencyProperty TagsProperty = DependencyProperty.Register(
		nameof(TagFilter.Tags),
		typeof(TagCollection),
		typeof(TagFilter),
		new(new TagCollection(), OnTagsChanged));

	public static readonly DependencyProperty SearchProperty = DependencyProperty.Register(
		nameof(TagFilter.Search),
		typeof(string),
		typeof(TagFilter),
		new(null));

	protected readonly ILogger Log = Logging.Shared.ForContext<TagFilter>();

	private readonly FuncQueue tagSearchQueue;
	private bool isChangingTags = false;

	public TagFilter()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.tagSearchQueue = new(this.SearchAsync, 250);
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	public event RoutedEventHandler? OnDone;

	public FastObservableCollection<Tag> SuggestTags { get; init; } = new();
	public Tag? SelectedSuggestTag { get; set; } = null;
	public int AvailableTagsExtra { get; private set; } = 0;
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

	protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}

	private static void OnTagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is TagFilter tagFilter)
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
		}

		Keyboard.Focus(this.SearchTextBox);
		this.SearchTextBox.Focus();
		this.SearchTextBox.CaretIndex = int.MaxValue;
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

		this.isChangingTags = false;
	}

	private void AddTag(Tag tag)
	{
		this.isChangingTags = true;

		this.SelectedTags.Add(tag);
		this.SuggestTags.Remove(tag);
		this.SuggestTags.Sort(this);
		this.Tags.Replace(this.SelectedTags);
		this.Search = string.Empty;

		this.isChangingTags = false;
	}

	private void OnAddTagMouseDown(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
	}

	private void OnAddTagMouseUp(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
		if (sender is FrameworkElement el && el.DataContext is Tag tag)
		{
			this.AddTag(tag);
		}
	}

	private void OnRemoveTagMouseDown(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
	}

	private void OnRemoveTagMouseUp(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
		if (sender is FrameworkElement el && el.DataContext is Tag tag)
		{
			this.RemoveTag(tag);
		}
	}

	private void OnTagSearchGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		this.tagSearchQueue.Invoke();
	}

	private void OnTagSearchLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		this.SuggestTags.Clear();
	}

	private void OnTagSearchPreviewKeyDown(object sender, KeyEventArgs e)
	{
		try
		{
			if (e.Key == Key.Escape || e.Key == Key.Return)
			{
				Keyboard.ClearFocus();
				e.Handled = true;

				this.OnDone?.Invoke(this, new());
			}
			else if (e.Key == Key.Tab)
			{
				if(this.SelectedSuggestTag != null)
					this.AddTag(this.SelectedSuggestTag);

				e.Handled = true;
			}
			else if (e.Key == Key.Down)
			{
				this.IncrementSuggestIndex(1);
				e.Handled = true;
			}
			else if (e.Key == Key.Up)
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

	private void IncrementSuggestIndex(int ammount)
	{
		int currentIndex = 0;

		if (this.SelectedSuggestTag != null)
		{
			currentIndex = this.SuggestTags.IndexOf(this.SelectedSuggestTag);

			if (currentIndex < 0)
			{
				this.SelectedSuggestTag = this.SuggestTags[0];
				this.NotifyPropertyChanged(nameof(TagFilter.SelectedSuggestTag));
				return;
			}
		}

		currentIndex += ammount;

		if (currentIndex < 0)
			currentIndex = this.SuggestTags.Count - 1;

		if (currentIndex >= this.SuggestTags.Count)
			currentIndex = 0;

		this.SelectedSuggestTag = this.SuggestTags[currentIndex];
		this.NotifyPropertyChanged(nameof(TagFilter.SelectedSuggestTag));
	}

	private async Task SearchAsync()
	{
		await this.Dispatcher.MainThread();
		string? str = this.Search;

		if (string.IsNullOrEmpty(str))
		{
			await this.Dispatcher.MainThread();
			this.SuggestTags.Clear();
			return;
		}

		HashSet<Tag> filterByTags = new(this.SelectedTags);
		HashSet<Tag>? allTags = this.AllTags != null ? new(this.AllTags) : null;

		await Dispatch.NonUiThread();

		string[]? querry = null;
		if (!string.IsNullOrEmpty(str))
		{
			if (str != null)
			{
				str = str.ToLower();
				querry = str.Split(' ');
			}
		}

		List<Tag> tags = new List<Tag>();
		if (allTags != null)
		{
			foreach (Tag tag in allTags)
			{
				if (filterByTags.Contains(tag))
					continue;

				if (querry != null && !tag.Search(querry))
					continue;

				tags.Add(tag);
			}

			tags.Sort(this);
		}

		this.AvailableTagsExtra = tags.Count;

		while (tags.Count > 9)
			tags.RemoveAt(9);

		this.AvailableTagsExtra -= tags.Count;
		this.AvailableTagsExtra = Math.Max(this.AvailableTagsExtra, 0);

		await this.Dispatcher.MainThread();
		this.SuggestTags.Replace(tags);

		if (this.SuggestTags.Count > 0)
		{
			this.SelectedSuggestTag = this.SuggestTags[0];
			this.NotifyPropertyChanged(nameof(TagFilter.SelectedSuggestTag));
		}

		this.NotifyPropertyChanged(nameof(TagFilter.AvailableTagsExtra));
	}
}