namespace ScreenshotStudio.Studio.Pose;

using ScreenshotStudio.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.LinkLabel;

public class PoseView : Canvas
{
	protected readonly ILogger Log = Logging.ForContext<PoseView>();

	private const double BackgroundOpacity = 0.25;

	private readonly PoseViewDefinition? definition;
	private readonly List<BoneLink> boneLinks = new();
	private readonly int backgroundWidth;
	private readonly int backgroundHeight;

	public PoseView()
	{
		if (DesignerProperties.GetIsInDesignMode(this))
			return;

		PoseViewDefinition? def = null;
		DataService.PoseViews?.TryGetValue("Body", out def);
		this.definition = def;

		if (def == null)
			return;

		// set background
		BitmapImage bmp = new();
		bmp.BeginInit();
		bmp.UriSource = new($"pack://application:,,,/ScreenshotStudio;component/{def.Background}");
		bmp.EndInit();

		this.backgroundWidth = bmp.PixelWidth;
		this.backgroundHeight = bmp.PixelHeight;

		ImageBrush brush = new();
		brush.ImageSource = bmp;
		brush.Stretch = Stretch.Uniform;
		brush.Opacity = BackgroundOpacity;
		brush.AlignmentX = AlignmentX.Left;
		brush.AlignmentY = AlignmentY.Top;
		this.Background = brush;

		// populate bones
		this.boneLinks.Clear();
		foreach ((string name, Point pos) in def.Bones)
		{
			this.boneLinks.Add(new(name, null, this));

			if (name.EndsWith("_l"))
			{
				string rName = name.Substring(0, name.Length - 2) + "_r";
				this.boneLinks.Add(new(rName, name, this));
			}
		}
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);

		if (this.definition == null)
			return;

		double scaleY = this.ActualHeight / (double)this.backgroundHeight;
		double scaleX = this.ActualWidth / (double)this.backgroundWidth;
		double scale = Math.Min(scaleX, scaleY);

		foreach(BoneLink link in this.boneLinks)
		{
			string lookupName = link.Name;
			bool isFlip = false;
			if (link.OriginalName != null)
			{
				isFlip = true;
				lookupName = link.OriginalName;
			}

			Point pos;
			if (!this.definition.Bones.TryGetValue(lookupName, out pos))
				continue;

			Point finalPos = default;
			if (isFlip)
			{
				finalPos.X = (this.backgroundWidth - pos.X) * scale;
			}
			else
			{
				finalPos.X = pos.X * scale;
			}

			finalPos.Y = pos.Y * scale;
			link.SetPosition(finalPos);
		}
	}

	public class BoneLink
	{
		public const double Radius = 7;

		public readonly string Name;
		public readonly string? OriginalName;

		private readonly Ellipse outer;

		public BoneLink(string name, string? originalName, Canvas parent)
		{
			this.Name = name;
			this.OriginalName = originalName;

			this.outer = new();
			this.outer.Width = Radius * 2;
			this.outer.Height = Radius * 2;
			this.outer.StrokeThickness = 1;
			this.outer.SetResourceReference(Ellipse.StrokeProperty, "ForegroundLightBrush");
			this.outer.SetResourceReference(Ellipse.FillProperty, "ControlBackgroundBrush");

			parent.Children.Add(this.outer);
		}

		public void SetPosition(Point pos)
		{
			Canvas.SetLeft(this.outer, pos.X - Radius);
			Canvas.SetTop(this.outer, pos.Y - Radius);
		}
	}
}

public class PoseViewDefinition
{
	public string? Background { get; set; }
	public Dictionary<string, Point> Bones { get; set; } = new();
}