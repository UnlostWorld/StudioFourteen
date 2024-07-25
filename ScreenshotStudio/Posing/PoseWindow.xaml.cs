namespace ScreenshotStudio.Studio;

using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using ScreenshotStudio.Data;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs.Extensions;
using ScreenshotStudio.Studio.Pose;
using ScreenshotStudio.Windows;
using System.Collections.Generic;
using System.Windows.Controls;

public partial class PoseWindow : CharacterWindow
{
	private hkQsTransformf fallback = default;

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
		get => this.Transform.Translation.X;
		set => this.Transform.Translation.X = (float)value;
	}

	[AutoNotify]
	public double TranslationY
	{
		get => this.Transform.Translation.Y;
		set => this.Transform.Translation.Y = (float)value;
	}

	[AutoNotify]
	public double TranslationZ
	{
		get => this.Transform.Translation.Z;
		set => this.Transform.Translation.Z = (float)value;
	}

	[AutoNotify]
	public double EulerRotationX
	{
		get => this.Transform.Rotation.ToEuler().X;
		set
		{
			hkVector4f euler = this.Transform.Rotation.ToEuler();
			euler.X = (float)value;
			this.Transform.Rotation.FromEuler(euler);
		}
	}

	[AutoNotify]
	public double EulerRotationY
	{
		get => this.Transform.Rotation.ToEuler().Y;
		set
		{
			hkVector4f euler = this.Transform.Rotation.ToEuler();
			euler.Y = (float)value;
			this.Transform.Rotation.FromEuler(euler);
		}
	}

	[AutoNotify]
	public double EulerRotationZ
	{
		get => this.Transform.Rotation.ToEuler().Z;
		set
		{
			hkVector4f euler = this.Transform.Rotation.ToEuler();
			euler.Z = (float)value;
			this.Transform.Rotation.FromEuler(euler);
		}
	}

	[AutoNotify]
	public double ScaleX
	{
		get => this.Transform.Scale.X;
		set => this.Transform.Scale.X = (float)value;
	}

	[AutoNotify]
	public double ScaleY
	{
		get => this.Transform.Scale.Y;
		set => this.Transform.Scale.Y = (float)value;
	}

	[AutoNotify]
	public double ScaleZ
	{
		get => this.Transform.Scale.Z;
		set => this.Transform.Scale.Z = (float)value;
	}

	[AutoNotify]
	public bool LockTransform
	{
		get => this.Selection?.LockTransform ?? false;
		set
		{
			if (this.Selection != null)
			{
				this.Selection.LockTransform = value;
			}
		}
	}

	private ref hkQsTransformf Transform
	{
		get
		{
			if (this.Selection == null)
				return ref this.fallback;

			return ref this.Selection.Transform;
		}
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