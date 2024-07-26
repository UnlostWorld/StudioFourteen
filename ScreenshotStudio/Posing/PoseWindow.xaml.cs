namespace ScreenshotStudio.Posing;

using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using ScreenshotStudio.Data;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs.Extensions;
using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class PoseWindow : CharacterWindow
{
	private Vector3? trackingEuler;

	public PoseEditModes[] EditModes => Enum.GetValues<PoseEditModes>();

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
		get => this.Translation.X;
		set
		{
			Vector3 translation = this.Translation;
			translation.X = (float)value;
			this.Translation = translation;
		}
	}

	[AutoNotify]
	public double TranslationY
	{
		get => this.Translation.Y;
		set
		{
			Vector3 translation = this.Translation;
			translation.Y = (float)value;
			this.Translation = translation;
		}
	}

	[AutoNotify]
	public double TranslationZ
	{
		get => this.Translation.Z;
		set
		{
			Vector3 translation = this.Translation;
			translation.Z = (float)value;
			this.Translation = translation;
		}
	}

	[AutoNotify]
	public double EulerRotationX
	{
		get => this.EulerRotation.X;
		set
		{
			Vector3 euler = this.EulerRotation;
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
			Vector3 euler = this.EulerRotation;
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
			Vector3 euler = this.EulerRotation;
			euler.Z = (float)value;
			this.EulerRotation = euler;
		}
	}

	public Vector3 EulerRotation
	{
		get
		{
			if (this.trackingEuler != null)
				return (Vector3)this.trackingEuler;

			return this.Rotation.ToEuler();
		}

		set
		{
			this.trackingEuler = value;

			Quaternion rotation = this.Rotation;
			rotation.FromEuler(value);
			this.Rotation = rotation;
		}
	}

	[AutoNotify]
	public double ScaleX
	{
		get => this.Scale.X;
		set
		{
			Vector3 scale = this.Scale;
			scale.X = (float)value;
			this.Scale = scale;
		}
	}

	[AutoNotify]
	public double ScaleY
	{
		get => this.Scale.Y;
		set
		{
			Vector3 scale = this.Scale;
			scale.Y = (float)value;
			this.Scale = scale;
		}
	}

	[AutoNotify]
	public double ScaleZ
	{
		get => this.Scale.Z;
		set
		{
			Vector3 scale = this.Scale;
			scale.Z = (float)value;
			this.Scale = scale;
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

	[AutoNotify]
	public Vector3 Translation
	{
		get => this.Selection?.Translation ?? default;
		set
		{
			if (this.Selection == null)
				return;

			this.Selection.Translation = value;
		}
	}

	[AutoNotify]
	public Quaternion Rotation
	{
		get => this.Selection?.Rotation ?? default;
		set
		{
			if (this.Selection == null)
				return;

			this.Selection.Rotation = value;
		}
	}

	[AutoNotify]
	public Vector3 Scale
	{
		get => this.Selection?.Scale ?? default;
		set
		{
			if (this.Selection == null)
				return;

			this.Selection.Scale = value;
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
		this.trackingEuler = this.Rotation.ToEuler();
	}

	private void OnEulerUp(object sender, MouseButtonEventArgs e)
	{
		this.trackingEuler = null;
	}
}