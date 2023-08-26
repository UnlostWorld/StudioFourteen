// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Tags;

using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf;
using XivToolsWpf.Converters;
using XivToolsWpf.DependencyProperties;
using XivToolsWpf.Extensions;
using XivToolsWpf.Utils;

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
		new(new TagCollection()));

	protected readonly ILogger Log = Logging.Shared.ForContext<TagFilter>();

	private readonly AddTag addTagItem = new();
	private readonly FuncQueue tagSearchQueue;
	private string? tagSearchText;

	public TagFilter()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.tagSearchQueue = new(this.SearchAsync, 250);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public FastObservableCollection<Tag> AvailableTags { get; init; } = new();
	public FastObservableCollection<Tag> FilterByTags { get; init; } = new();

	public SearchTag SearchTag { get; init; } = new(string.Empty);
	public Tag? SuggestTag { get; set; }

	public string? TagSearchText
	{
		get => this.tagSearchText;
		set
		{
			this.tagSearchText = value;
			this.SearchTag.Name = value;

			this.PropertyChanged?.Invoke(this, new(nameof(TagFilter.TagSearchText)));
		}
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

	public bool HasFocus { get; private set; }

	public int Compare(Tag? x, Tag? y) => string.Compare(x?.Name, y?.Name);

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (!this.FilterByTags.Contains(this.addTagItem))
		{
			this.FilterByTags.Add(this.addTagItem);
		}

		if (this.AvailableTags != null)
		{
			this.AvailableTags.SortAndReplace(this.AvailableTags, this);
		}
	}

	private async void RemoveTag(Tag tag)
	{
		this.FilterByTags.Remove(tag);
		this.AvailableTags.Add(tag);
		this.AvailableTags.Sort(this);

		this.Tags.Replace(this.FilterByTags);

		if (this.FilterByTags.Count <= 1)
		{
			this.addTagItem.ShowHint = true;
			this.addTagItem.IsSelected = true;
			await Task.Delay(50);
			this.addTagItem.IsSelected = false;
		}
	}

	private void AddTag(Tag tag)
	{
		this.addTagItem.ShowHint = false;

		this.FilterByTags.Insert(this.FilterByTags.Count - 1, tag);

		this.AvailableTags.Remove(tag);
		this.AvailableTags.Sort(this);

		this.Tags.Replace(this.FilterByTags);
	}

	private void OnTagClicked(object sender, RoutedEventArgs e)
	{
		try
		{
			if (sender is Button btn && btn.DataContext is Tag tag)
			{
				if (this.FilterByTags.Contains(tag))
				{
					this.RemoveTag(tag);
				}
				else
				{
					this.AddTag(tag);
				}
			}
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error in OnTagClicked");
		}
	}

	private void OnTagSearchGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		this.HasFocus = true;
		this.PropertyChanged?.Invoke(this, new(nameof(TagFilter.HasFocus)));

		this.tagSearchQueue.Invoke();
	}

	private void OnTagSearchLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		this.HasFocus = false;
		this.PropertyChanged?.Invoke(this, new(nameof(TagFilter.HasFocus)));
	}

	private async void OnTagSearchPreviewKeyDown(object sender, KeyEventArgs e)
	{
		try
		{
			if (e.Key == Key.Escape)
			{
				this.TagSearchText = null;
				Keyboard.ClearFocus();
				return;
			}
			else if (e.Key == Key.Return)
			{
				if (string.IsNullOrEmpty(this.SearchTag.Name) || string.IsNullOrEmpty(this.TagSearchText))
					return;

				this.AddTag(new SearchTag(this.SearchTag.Name));
				this.TagSearchText = null;
				this.SearchTag.Query = string.Empty;
			}
			else if (e.Key == Key.Tab)
			{
				e.Handled = true;

				this.tagSearchQueue.InvokeImmediate();
				await this.tagSearchQueue.WaitForPendingExecute();

				if (this.AvailableTags.Count > 0)
				{
					this.AddTag(this.AvailableTags[0]);
				}

				// clear the serach and run it again so the next time we open we have blank results.
				this.TagSearchText = null;
				this.tagSearchQueue.InvokeImmediate();
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

	private async Task SearchAsync()
	{
		await this.Dispatcher.MainThread();
		string? str = this.TagSearchText;
		HashSet<Tag> filterByTags = new(this.FilterByTags);
		HashSet<Tag>? allTags = this.AllTags != null ? new(this.AllTags) : null;
		await Dispatch.NonUiThread();

		string[]? querry = null;
		if (str != null)
		{
			str = str.ToLower();
			querry = str.Split(' ');
		}

		List<Tag> tags = new List<Tag>();
		if (allTags != null)
		{
			foreach (Tag tag in allTags)
			{
				if (filterByTags.Contains(tag))
					continue;

				if (!tag.Search(querry))
					continue;

				tags.Add(tag);
			}
		}

		await this.Dispatcher.MainThread();

		this.AvailableTags.SortAndReplace(tags, this);
		this.SuggestTag = this.AvailableTags.FirstOrDefault();
		this.PropertyChanged?.Invoke(this, new(nameof(this.SuggestTag)));
	}
}

public class AddTag : Tag, INotifyPropertyChanged
{
	private bool showHint = true;
	private bool isSelected = false;

	public AddTag()
		: base("New Tag")
	{
	}

	public bool ShowHint
	{
		get => this.showHint;
		set
		{
			this.showHint = value;
			this.NotifyPropertyChanged();
		}
	}

	public bool IsSelected
	{
		get => this.isSelected;
		set
		{
			this.isSelected = value;
			this.NotifyPropertyChanged();
		}
	}

	public override bool Search(string[]? querry) => false;
}

// This is aweful, but if the button is a child of the FilterTagsControl then its for removing tags,
// otherwise its for adding.
public class CanTagAddConverter : ConverterBase<Button, Visibility>
{
	protected override Visibility Convert(Button? value)
	{
		if (value == null)
			return Visibility.Collapsed;

		bool isAdding = this.IsButtonForAddingTags(value);

		if (this.Parameter is string str && str == "Remove")
			isAdding = !isAdding;

		return isAdding ? Visibility.Visible : Visibility.Collapsed;
	}

	protected bool IsButtonForAddingTags(Button value)
	{
		ItemsControl? host = value.FindParent<ItemsControl>();
		if (host?.Name == "FilterTagsControl")
		{
			return false;
		}

		return true;
	}
}