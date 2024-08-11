namespace ScreenshotStudio.Windows;

using DependencyPropertyGenerator;
using FontAwesome.Sharp;
using ScreenshotStudio.Utilities;
using System;
using System.Windows;
using WpfUtils.Extensions;

[DependencyProperty<IconChar>("TitleIcon")]
[DependencyProperty<bool>("CanClose", DefaultValue = true)]
[DependencyProperty<bool>("CanChangeEmbed", DefaultValue = true)]
[DependencyProperty<string>("Subtitle")]
[DependencyProperty<double>("Scale", DefaultValue = 1.0)]
[DependencyProperty<bool>("IsMaximized", DefaultValue = false)]
public partial class PanelWindow : PersistentPanel
{
	private double preScaleHeight;
	private double preScaleWidth;

	public virtual Point? SavedPosition
	{
		get => this.GetPersistence<Point?>();
		set => this.SetPersistence(value);
	}

	public virtual double SavedScale
	{
		get => this.GetPersistence<double?>() ?? 1.0;
		set => this.SetPersistence(value);
	}

	public Point Position
	{
		get => XivWindow.GetPosition(this);
		set => XivWindow.SetPosition(this, value);
	}

	public FastObservableCollection<double> ZoomOptions { get; init; } = new()
	{
		0.5,
		0.75,
		1.0,
		1.25,
		1.5,
		2.0,
		2.5,
	};

	protected override Style GetDefaultStyle() => (Style)this.FindResource("PanelWindowStyle");

	protected override void OnOpened()
	{
		this.preScaleHeight = this.Height;
		this.preScaleWidth = this.Width;

		this.Scale = this.SavedScale;

		if (this.SavedPosition != null)
			this.Position = (Point)this.SavedPosition;

		if (XivWindow.Process == null)
			this.CanChangeEmbed = false;

		base.OnOpened();
	}

	protected override void OnClosed()
	{
		base.OnClosed();
		this.SavedPosition = this.Position;
	}

	protected override void OnStateChanged(EventArgs e)
	{
		base.OnStateChanged(e);
		this.IsMaximized = this.WindowState == WindowState.Maximized;
	}

	partial void OnIsMaximizedChanged(bool newValue)
	{
		this.WindowState = newValue ? WindowState.Maximized : WindowState.Normal;
	}

	partial void OnScaleChanged()
	{
		this.SavedScale = this.Scale;

		if (this.ResizeMode == ResizeMode.NoResize)
		{
			if (this.SizeToContent == SizeToContent.Width)
			{
				this.Height = this.preScaleHeight * this.Scale;
			}
			else if (this.SizeToContent == SizeToContent.Height)
			{
				this.Width = this.preScaleWidth * this.Scale;
			}
		}
	}
}