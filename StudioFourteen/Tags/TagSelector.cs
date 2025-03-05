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

namespace StudioFourteen.Tags;

using Dalamud.Utility;
using DependencyPropertyGenerator;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using WpfUtils;
using WpfUtils.Utils;

[DependencyProperty<TagCollection>("Tags")]
[DependencyProperty<TagCollection>("SelectedTags")]
[DependencyProperty<TagCollection>("SearchTags")]
[DependencyProperty<bool>("PopOutOpen", DefaultBindingMode = DefaultBindingMode.TwoWay)]
public partial class TagSelector : Control
{
	private readonly FuncQueue searchQueue;
	private TagsControl? currentTagDisplay;
	private TagsControl? newTagDisplay;
	private TextBox? searchBox;

	public TagSelector()
	{
		this.searchQueue = new(this.Search, 250);
	}

	public override void OnApplyTemplate()
	{
		if (this.newTagDisplay != null)
			this.newTagDisplay.TagSelected -= this.OnNewTagSelected;

		if (this.currentTagDisplay != null)
			this.currentTagDisplay.TagSelected -= this.OnCurrentTagSelected;

		if (this.searchBox != null)
			this.searchBox.TextChanged -= this.OnSearchTextChanged;

		base.OnApplyTemplate();
		this.newTagDisplay = this.GetTemplateChild("PART_PopOutTagDisplay") as TagsControl;
		this.currentTagDisplay = this.GetTemplateChild("PART_PopOutSelectedTagDisplay") as TagsControl;
		this.searchBox = this.GetTemplateChild("PART_SearchBox") as TextBox;

		if (this.currentTagDisplay != null)
			this.currentTagDisplay.TagSelected += this.OnCurrentTagSelected;

		if (this.newTagDisplay != null)
			this.newTagDisplay.TagSelected += this.OnNewTagSelected;

		if (this.searchBox != null)
			this.searchBox.TextChanged += this.OnSearchTextChanged;
	}

	private void OnCurrentTagSelected(Tag tag)
	{
		this.SelectedTags?.Remove(tag);
		this.SearchTags?.Add(tag);
	}

	private void OnNewTagSelected(Tag tag)
	{
		this.SearchTags?.Remove(tag);
		this.SelectedTags?.Add(tag);
	}

	partial void OnPopOutOpenChanged(bool newValue)
	{
		if (this.searchBox == null)
			return;

		if (!newValue)
			return;

		this.searchBox.Text = string.Empty;
		this.searchQueue.InvokeImmediate();

		this.Dispatcher.InvokeAsync(async () =>
		{
			await Task.Delay(100);
			await this.MainThread();
			this.searchBox.Focus();
		});
	}

	private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
	{
		this.searchQueue.Invoke();
	}

	private async Task Search()
	{
		await this.MainThread();

		if (this.Tags == null)
			return;

		if (this.SearchTags == null)
			this.SearchTags = new();

		if (string.IsNullOrEmpty(this.searchBox?.Text))
		{
			this.SearchTags.Replace(this.Tags);
		}
		else
		{
			string[] query = SearchUtility.ToQuery(this.searchBox.Text ?? string.Empty);

			List<Tag> results = new();
			foreach (Tag tag in this.Tags)
			{
				if (tag.Search(query))
				{
					results.Add(tag);
				}
			}

			this.SearchTags.Replace(results);
		}
	}
}