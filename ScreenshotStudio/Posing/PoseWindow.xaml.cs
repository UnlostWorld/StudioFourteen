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

	[AutoNotify] public SelectionBase? Selection => this.Services.Pose.Selection;

	[AutoNotify]
	public double TranslationX
	{
		get => this.Selection?.Translation.X ?? 0;
		set { }
	}

	[AutoNotify]
	public double TranslationY
	{
		get => this.Selection?.Translation.Y ?? 0;
		set { }
	}

	[AutoNotify]
	public double TranslationZ
	{
		get => this.Selection?.Translation.Z ?? 0;
		set { }
	}

	[AutoNotify]
	public double EulerRotationX
	{
		get => this.Selection?.EulerRotation.X ?? 0;
		set { }
	}

	[AutoNotify]
	public double EulerRotationY
	{
		get => this.Selection?.EulerRotation.Y ?? 0;
		set { }
	}

	[AutoNotify]
	public double EulerRotationZ
	{
		get => this.Selection?.EulerRotation.Z ?? 0;
		set { }
	}

	[AutoNotify]
	public double ScaleX
	{
		get => this.Selection?.Scale.X ?? 0;
		set { }
	}

	[AutoNotify]
	public double ScaleY
	{
		get => this.Selection?.Scale.Y ?? 0;
		set { }
	}

	[AutoNotify]
	public double ScaleZ
	{
		get => this.Selection?.Scale.Z ?? 0;
		set { }
	}

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