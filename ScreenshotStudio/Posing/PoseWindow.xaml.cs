namespace ScreenshotStudio.Studio;

using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using ScreenshotStudio.Data;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs.Extensions;
using ScreenshotStudio.Studio.Pose;
using ScreenshotStudio.Windows;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class PoseWindow : CharacterWindow
{
	private hkVector4f? trackingEuler;

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
		set
		{
			var transform = this.Transform;
			transform.Translation.X = (float)value;
			this.Transform = transform;
		}
	}

	[AutoNotify]
	public double TranslationY
	{
		get => this.Transform.Translation.Y;
		set
		{
			var transform = this.Transform;
			transform.Translation.Y = (float)value;
			this.Transform = transform;
		}
	}

	[AutoNotify]
	public double TranslationZ
	{
		get => this.Transform.Translation.Z;
		set
		{
			var transform = this.Transform;
			transform.Translation.Z = (float)value;
			this.Transform = transform;
		}
	}

	[AutoNotify]
	public double EulerRotationX
	{
		get => this.EulerRotation.X;
		set
		{
			hkVector4f euler = this.EulerRotation;
			euler.X = (float)value;
			this.EulerRotation = euler;
		}
	}

	[AutoNotify]
	public double EulerRotationY
	{
		get => this.EulerRotation.Y;
		set
		{
			hkVector4f euler = this.EulerRotation;
			euler.Y = (float)value;
			this.EulerRotation = euler;
		}
	}

	[AutoNotify]
	public double EulerRotationZ
	{
		get => this.EulerRotation.Z;
		set
		{
			hkVector4f euler = this.EulerRotation;
			euler.Z = (float)value;
			this.EulerRotation = euler;
		}
	}

	public hkVector4f EulerRotation
	{
		get
		{
			if (this.trackingEuler != null)
				return (hkVector4f)this.trackingEuler;

			return this.Transform.Rotation.ToEuler();
		}

		set
		{
			this.trackingEuler = value;

			var transform = this.Transform;
			transform.Rotation.FromEuler(value);
			this.Transform = transform;
		}
	}

	[AutoNotify]
	public double ScaleX
	{
		get => this.Transform.Scale.X;
		set
		{
			var transform = this.Transform;
			transform.Scale.X = (float)value;
			this.Transform = transform;
		}
	}

	[AutoNotify]
	public double ScaleY
	{
		get => this.Transform.Scale.Y;
		set
		{
			var transform = this.Transform;
			transform.Scale.Y = (float)value;
			this.Transform = transform;
		}
	}

	[AutoNotify]
	public double ScaleZ
	{
		get => this.Transform.Scale.Z;
		set
		{
			var transform = this.Transform;
			transform.Scale.Z = (float)value;
			this.Transform = transform;
		}
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

	private hkQsTransformf Transform
	{
		get
		{
			if (this.Selection == null)
				return default;

			return this.Selection.Transform;
		}

		set
		{
			if (this.Selection == null)
				return;

			this.Selection.Transform = value;
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

	private void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Pose.FlushBoneReferences(this.TargetObjectIndex);
	}

	private void OnEulerDown(object sender, MouseButtonEventArgs e)
	{
		this.trackingEuler = this.Selection?.Transform.Rotation.ToEuler();
	}

	private void OnEulerUp(object sender, MouseButtonEventArgs e)
	{
		this.trackingEuler = null;
	}
}