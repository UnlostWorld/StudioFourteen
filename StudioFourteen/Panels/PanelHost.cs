// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Panels;

using DependencyPropertyGenerator;

using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;

[DependencyProperty<Type>("PanelType")]
public partial class PanelHost : ContentControl, Panel.IHost
{
	private Panel? panel;

	public PanelHost()
	{
		this.Unloaded += this.OnUnloaded;
		this.Dispatcher.ShutdownStarted += this.OnShutdownStarted;
		this.IsVisibleChanged += this.OnIsVisibleChanged;
	}

	void Panel.IHost.Close()
	{
		this.panel?.SetIsOpen(this, false);
	}

	partial void OnPanelTypeChanged(Type? newValue)
	{
		if (DesignerProperties.GetIsInDesignMode(this))
			return;

		this.panel?.SetIsOpen(this, false);
		this.Content = null;

		if (newValue == null)
			return;

		this.panel = Activator.CreateInstance(newValue) as Panel;

		if (panel != null)
		{
			this.panel.SetHost(this);
			this.Content = this.panel;
			this.panel.SetIsOpen(this, this.IsVisible);

			this.panel.Width = double.NaN;
			this.panel.Height = double.NaN;
			this.panel.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.panel.VerticalAlignment = VerticalAlignment.Stretch;
		}
	}

	private void OnShutdownStarted(object? sender, EventArgs e)
	{
		this.panel?.SetIsOpen(this, false);
	}

	private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
	{
		this.panel?.SetIsOpen(this, false);
	}

	private void OnIsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
	{
		this.panel?.SetIsOpen(this, this.IsVisible);
	}
}
