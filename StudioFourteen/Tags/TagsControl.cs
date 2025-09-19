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
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using StudioFourteen.Extensions;

[DependencyProperty<TagCollection>("Tags")]
[DependencyProperty<ItemsPanelTemplate>("ItemsPanelTemplate")]
[DependencyProperty<int>("InvisibleCount")]
public partial class TagsControl : Control
{
	private ItemsControl? itemsControl;

	public TagsControl()
	{
		this.OnTagsChanged(null, this.Tags);
	}

	public delegate void TagSelectedDelegate(Tag tag);

	public event TagSelectedDelegate? TagSelected;

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		this.itemsControl = this.GetTemplateChild("PART_ItemsControl") as ItemsControl;
	}

	public void OnTagClicked(Tag tag)
	{
		this.TagSelected?.Invoke(tag);
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);
		this.UpdateInvisibleCount();
	}

	partial void OnTagsChanged(TagCollection? oldValue, TagCollection? newValue)
	{
		if (oldValue != null)
		{
			oldValue.CollectionChanged -= this.OnValueCollectionChanged;
		}

		if (newValue != null)
		{
			newValue.CollectionChanged += this.OnValueCollectionChanged;
		}
	}

	private void OnValueCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		Task.Run(async () =>
		{
			await Task.Delay(10);
			await this.Dispatcher.BeginInvoke(() => this.UpdateInvisibleCount());
		});
	}

	private void UpdateInvisibleCount()
	{
		if (this.itemsControl == null || this.Tags == null)
			return;

		int oldCount = this.InvisibleCount;

		int visibleTagsCount = this.itemsControl.GetVisibleItemsCount();
		this.InvisibleCount = this.Tags.Count - visibleTagsCount;
		this.itemsControl.HideClippedItems();

		// Since the number appearing in the UI can cause more tags to become clipped,
		// run the check again when we go from 0 to 1 for the first time.
		if (this.InvisibleCount > 0 && oldCount <= 0)
		{
			Task.Run(async () =>
			{
				await Task.Delay(10);
				await this.Dispatcher.BeginInvoke(() => this.UpdateInvisibleCount());
			});
		}
	}
}