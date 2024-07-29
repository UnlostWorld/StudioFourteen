namespace ScreenshotStudio.Windows;

using FontAwesome.Sharp.Pro;
using ScreenshotStudio.Utilities;
using System;
using System.Windows;
using System.Windows.Input;
using WpfUtils.Commands;

public class PanelWindow : PersistentPanel
{
	public static readonly DependencyProperty TitleIconProperty = DependencyProperty.Register(
		nameof(PanelWindow.TitleIcon),
		typeof(ProIcons),
		typeof(PanelWindow));

	public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register(
		nameof(PanelWindow.CanClose),
		typeof(bool),
		typeof(PanelWindow),
		new(true));

	public static readonly DependencyProperty CanChangeEmbedProperty = DependencyProperty.Register(
		nameof(PanelWindow.CanChangeEmbed),
		typeof(bool),
		typeof(PanelWindow),
		new(true));

	public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
		nameof(PanelWindow.Subtitle),
		typeof(string),
		typeof(PanelWindow));

	public ProIcons TitleIcon
	{
		get => (ProIcons)this.GetValue(TitleIconProperty);
		set => this.SetValue(TitleIconProperty, value);
	}

	public bool CanClose
	{
		get => (bool)this.GetValue(CanCloseProperty);
		set => this.SetValue(CanCloseProperty, value);
	}

	public bool CanChangeEmbed
	{
		get => (bool)this.GetValue(CanChangeEmbedProperty);
		set => this.SetValue(CanChangeEmbedProperty, value);
	}

	public string Subtitle
	{
		get => (string)this.GetValue(SubtitleProperty);
		set => this.SetValue(SubtitleProperty, value);
	}

	public virtual Point? SavedPosition
	{
		get => this.GetPersistence<Point?>();
		set => this.SetPersistence(value);
	}

	public Point Position
	{
		get => XivWindow.GetPosition(this);
		set => XivWindow.SetPosition(this, value);
	}

	protected override Style GetDefaultStyle() => (Style)this.FindResource("PanelWindowStyle");

	protected override void OnOpened()
	{
		if (this.SavedPosition != null)
			this.Position = (Point)this.SavedPosition;

		base.OnOpened();
	}

	protected override void OnClosed()
	{
		base.OnClosed();
		this.SavedPosition = this.Position;
	}
}

public class PanelWindowAction
{
	public PanelWindowAction()
	{
	}

	public PanelWindowAction(ProIcons icon, string? tooltip, Action callback)
	{
		this.Icon = icon;
		this.ToolTip = tooltip;
		this.Command = new SimpleCommand(callback);
	}

	public string? ToolTip { get; set; }
	public ProIcons Icon { get; set; } = ProIcons.None;
	public ICommand? Command { get; set; }
}