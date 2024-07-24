namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Data;
using ScreenshotStudio.Services;
using ScreenshotStudio.Studio.Pose;
using ScreenshotStudio.Windows;
using System.Collections.Generic;
using System.Windows.Controls;

public partial class PoseWindow : CharacterWindow
{
	[AutoNotify]
	public bool ExpandTranslationSliders
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	[AutoNotify]
	public bool ExpandRotationSliders
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	[AutoNotify]
	public bool ExpandScaleSliders
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	public double TranslationX { get; set; }
	public double TranslationY { get; set; }
	public double TranslationZ { get; set; }

	public double EulerRotationX { get; set; }
	public double EulerRotationY { get; set; }
	public double EulerRotationZ { get; set; }

	public double ScaleX { get; set; }
	public double ScaleY { get; set; }
	public double ScaleZ { get; set; }

	protected override void OnOpened()
	{
		base.OnOpened();

		if (DataService.SkeletonViews == null)
			return;

		Dictionary<string, WrapPanel> panels = new();
		int categoryIndex = 0;
		foreach(PoseViewDefinition def in DataService.SkeletonViews)
		{
			if (def.Category == null)
				continue;

			if (!panels.ContainsKey(def.Category))
			{
				WrapPanel panel = new();
				panel.Orientation = Orientation.Vertical;
				panel.Height = 512;

				TabItem item = new();
				item.Header = def.Category; // localize me!
				item.Content = panel;
				this.GuiTabs.Items.Insert(categoryIndex, item);

				panels.Add(def.Category, panel);
				categoryIndex++;
			}

			SkeletonView view = new();
			view.ViewDefinition = def;
			panels[def.Category].Children.Add(view);
		}

		this.GuiTabs.SelectedIndex = 0;
	}
}