// © XivTools.
// Licensed under the MIT license.

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

	protected override Style GetDefaultStyle() => (Style)this.FindResource("PanelWindowStyle");
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