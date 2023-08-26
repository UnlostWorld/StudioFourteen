// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Library;

using ScreenshotStudio.Tags;
using ScreenshotStudio.Windows;
using System.Threading.Tasks;
using System.Windows;

public partial class QuickSearch : PanelWindow
{
	private static QuickSearch? instance;

	public QuickSearch()
	{
		instance = this;
		this.InitializeComponent();
	}

	public string SearchTitle { get; private set; } = "Library Search";
	public TagCollection? AvailableTags { get; private set; }
	public TagCollection? Tags { get; set; }

	public static void Show<T>(object placementTarget, string title, TagCollection defaultTags)
			where T : ILibraryItem
	{
		if (placementTarget is UIElement el)
		{
			Show<T>(el, title, defaultTags);
		}
	}

	public static void Show<T>(UIElement placementTarget, string title, TagCollection defaultTags)
		where T : ILibraryItem
	{
		if (instance == null)
		{
			Task.Run(async () =>
			{
				await Panel.ShowAsync<QuickSearch>();
				instance?.OnShow<T>(placementTarget, title, defaultTags);
			});
		}
		else
		{
			instance.OnShow<T>(placementTarget, title, defaultTags);
		}
	}

	public void OnShow<T>(UIElement placementTarget, string title, TagCollection defaultTags)
		where T : ILibraryItem
	{
		this.Tags = defaultTags;
		this.NotifyPropertyChanged(nameof(QuickSearch.Tags));

		this.SearchTitle = title;
		this.NotifyPropertyChanged(nameof(QuickSearch.SearchTitle));

		this.AvailableTags = this.Services.Library.GetAvailableTags<T>();
		this.NotifyPropertyChanged(nameof(QuickSearch.AvailableTags));
	}

	protected override void OnClosed()
	{
		base.OnClosed();
		instance = null;
	}
}
