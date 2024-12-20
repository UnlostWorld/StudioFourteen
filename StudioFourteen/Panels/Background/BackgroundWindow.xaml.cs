// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Studio;

using StudioFourteen.Panels;
using StudioFourteen.Settings;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfUtils.Extensions;

public partial class BackgroundWindow : PanelWindow
{
	public static BackgroundWindow? Instance;

	public Persistence Persistence = new("Panel_BackgroundWindow");

	public BackgroundWindow()
	{
		Instance = this;
		this.ContentArea.DataContext = this;
	}

	public override bool CanNavigate => false;

	protected override void OnOpened()
	{
		base.OnOpened();
		this.UpdatePosition();
	}

	protected override void OnActivated(EventArgs e)
	{
		base.OnActivated(e);
		this.Services.Windows.SendToBack(this);
	}

	protected override void OnDeactivated(EventArgs e)
	{
		base.OnDeactivated(e);
		this.Services.Windows.SendToBack(this);
	}

	protected override void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (this.Services.Input.Mouse?.IsAnyDragging == false)
		{
			if (e.ChangedButton == MouseButton.Right)
			{
				this.WorldContextMenu.Show(e.GetPosition(this));
			}
			else if (e.ChangedButton == MouseButton.Left)
			{
				this.Services.Target.TargetPosition(e.GetPosition(this)).Run();
			}
		}

		Point point = e.GetPosition(this);
		this.Services.Input.Mouse?.HandleMouse(e.ChangedButton, false, new Vector2((float)point.X, (float)point.Y));
	}

	protected override void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		Point point = e.GetPosition(this);
		this.Services.Input.Mouse?.HandleMouse(e.ChangedButton, true, new Vector2((float)point.X, (float)point.Y));
	}

	private void UpdatePosition()
	{
		if (this.Services.Windows.XivProcess == null)
			return;

		Rect xivWindowSize = this.Services.Windows.GetXivWindowClientSize();

		this.Width = xivWindowSize.Width;
		this.Height = xivWindowSize.Height;

		this.Services.Windows.SetPosition(this, new(0, 0));
	}

	private void OnShutdownClicked(object sender, RoutedEventArgs e)
	{
		Task.Run(this.Services.Stop);
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		Point mousePos = e.GetPosition(this);
		this.Services.Input.Mouse?.HandleMouseMove(new((float)mousePos.X, (float)mousePos.Y));
		this.Services.Windows.SendToBack(this);
	}

	private void OnMouseEnter(object sender, MouseEventArgs e)
	{
	}

	private void OnMouseLeave(object sender, MouseEventArgs e)
	{
		this.Services.Input.Mouse?.HandleMouseLeave();
	}

	private void OnMouseWheel(object sender, MouseWheelEventArgs e)
	{
		this.Services.Input.Mouse?.HandleMouseWheel(e.Delta / 120.0f);
		e.Handled = true;
	}

	private void OnMouseEnterSelf(object sender, MouseEventArgs e)
	{
		this.UpdatePosition();
	}

	private void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.IsRepeat)
			return;

		this.Services.Input.Keyboard?.HandleKey(e.Key, true);
		e.Handled = true;
	}

	private void OnPreviewKeyUp(object sender, KeyEventArgs e)
	{
		if (e.IsRepeat)
			return;

		this.Services.Input.Keyboard?.HandleKey(e.Key, false);
		e.Handled = true;
	}
}
