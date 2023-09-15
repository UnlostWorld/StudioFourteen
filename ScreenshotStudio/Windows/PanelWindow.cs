// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Windows;

using FontAwesome.Sharp.Pro;
using ScreenshotStudio.Utilities;
using System;
using System.Windows;
using System.Windows.Input;
using XivToolsWpf.Commands;
using XivToolsWpf.Extensions;

public class PanelWindow : PersistentPanel
{
	public static readonly DependencyProperty TitleIconProperty = DependencyProperty.Register(
		nameof(PanelWindow.TitleIcon),
		typeof(ProIcons),
		typeof(PanelWindow));

	public static readonly DependencyProperty ActionsProperty = DependencyProperty.Register(
		nameof(PanelWindow.Actions),
		typeof(FastObservableCollection<PanelWindowAction>),
		typeof(PanelWindow),
		new(new FastObservableCollection<PanelWindowAction>()));

	public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register(
		nameof(PanelWindow.CanClose),
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

	public FastObservableCollection<PanelWindowAction> Actions
	{
		get => (FastObservableCollection<PanelWindowAction>)this.GetValue(ActionsProperty);
		set => this.SetValue(ActionsProperty, value);
	}

	public bool CanClose
	{
		get => (bool)this.GetValue(CanCloseProperty);
		set => this.SetValue(CanCloseProperty, value);
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