

namespace ScreenshotStudio.Windows;

using FontAwesome.Sharp.Pro;
using System;
using System.Windows;
using System.Windows.Input;
using XivToolsWpf.Commands;
using XivToolsWpf.Extensions;

public class PanelWindow : Panel
{
	public static readonly DependencyProperty TitleIconProperty = DependencyProperty.Register(
		"TitleIcon",
		typeof(ProIcons),
		typeof(Panel));

	public static readonly DependencyProperty ActionsProperty = DependencyProperty.Register(
		"Actions",
		typeof(FastObservableCollection<PanelWindowAction>),
		typeof(Panel),
		new(new FastObservableCollection<PanelWindowAction>()));

	public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register(
		"CanClose",
		typeof(bool),
		typeof(Panel),
		new(true));

	public ProIcons TitleIcon
	{
		get => (ProIcons)GetValue(TitleIconProperty);
		set => SetValue(TitleIconProperty, value);
	}

	public FastObservableCollection<PanelWindowAction> Actions
	{
		get => (FastObservableCollection<PanelWindowAction>)GetValue(ActionsProperty);
		set => SetValue(ActionsProperty, value);
	}

	public bool CanClose
	{
		get => (bool)GetValue(CanCloseProperty);
		set => SetValue(CanCloseProperty, value);
	}

	protected override Style GetDefaultStyle() => (Style)this.FindResource("PanelWindowStyle");
}

public class PanelWindowAction
{
	public string? ToolTip { get; set; }
	public ProIcons Icon { get; set; } = ProIcons.None;
	public ICommand? Command { get; set; }

	public PanelWindowAction()
	{
	}

	public PanelWindowAction(ProIcons icon, string? tooltip, Action callback)
	{
		this.Icon = icon;
		this.ToolTip = tooltip;
		this.Command = new SimpleCommand(callback);
	}
}