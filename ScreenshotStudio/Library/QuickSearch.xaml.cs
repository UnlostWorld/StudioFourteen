// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Library;

using ScreenshotStudio.Services;
using ScreenshotStudio.Windows;
using System.ComponentModel;
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

	[AutoNotify] public string SearchTitle { get; private set; } = "Library Search";

	public static void Show(object placementTarget, string title)
	{
		if (placementTarget is UIElement el)
		{
			Show(el, title);
		}
	}

	public static void Show(UIElement placementTarget, string title)
	{
		if (instance == null)
		{
			Task.Run(async () =>
			{
				await Panel.ShowAsync<QuickSearch>();
				instance?.OnShow(placementTarget, title);
			});
		}
		else
		{
			instance.OnShow(placementTarget, title);
		}
	}

	public void OnShow(UIElement placementTarget, string title)
	{
		this.SearchTitle = title;
	}

	protected override void OnClosed()
	{
		base.OnClosed();
		instance = null;
	}
}
