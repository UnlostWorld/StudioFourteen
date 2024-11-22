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

using DependencyPropertyGenerator;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfUtils;
using WpfUtils.Utils;

[DependencyProperty<TagCollection>("Tags")]
[DependencyProperty<TagCollection>("SelectedTags")]
[DependencyProperty<string>("SearchString", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<TagCollection>("SearchTags")]
[DependencyProperty<bool>("PopOutOpen", DefaultBindingMode = DefaultBindingMode.TwoWay)]
public partial class TagSelector2 : Control
{
	private readonly FuncQueue searchQueue;
	private TagDisplay? newTagDisplay;
	private TagDisplay? tagDisplay;
	private TextBox? searchBox;

	public TagSelector2()
	{
		this.searchQueue = new(this.Search, 250);
	}

	public override void OnApplyTemplate()
	{
		if (this.newTagDisplay != null)
			this.newTagDisplay.TagSelected -= this.OnNewTagSelected;

		if (this.tagDisplay != null)
			this.tagDisplay.TagSelected -= this.OnTagSelected;

		base.OnApplyTemplate();
		this.newTagDisplay = this.GetTemplateChild("PART_PopOutTagDisplay") as TagDisplay;
		this.tagDisplay = this.GetTemplateChild("PART_TagDisplay") as TagDisplay;
		this.searchBox = this.GetTemplateChild("PART_SearchBox") as TextBox;

		if (this.newTagDisplay != null)
			this.newTagDisplay.TagSelected += this.OnNewTagSelected;

		if (this.tagDisplay != null)
			this.tagDisplay.TagSelected += this.OnTagSelected;
	}

	private void OnNewTagSelected(Tag tag)
	{
		this.SelectedTags?.Add(tag);
	}

	private void OnTagSelected(Tag tag)
	{
		this.SelectedTags?.Remove(tag);
	}

	partial void OnSearchStringChanged(string? newValue)
	{
		this.searchQueue.Invoke();
	}

	partial void OnPopOutOpenChanged(bool newValue)
	{
		if (!newValue)
			return;

		this.SearchString = string.Empty;
		this.searchQueue.InvokeImmediate();

		this.Dispatcher.InvokeAsync(async () =>
		{
			await Task.Delay(100);
			await this.MainThread();
			this.searchBox?.Focus();
		});
	}

	partial void OnSearchStringChanged()
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

		if (string.IsNullOrEmpty(this.SearchString))
		{
			this.SearchTags.Replace(this.Tags);
		}
		else
		{
			string[] query = SearchUtility.ToQuery(this.SearchString ?? string.Empty);

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