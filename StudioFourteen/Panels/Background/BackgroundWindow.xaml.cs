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

	private void UpdatePosition()
	{
		if (this.Services.Windows.XivProcess == null)
			return;

		Rect xivWindowSize = this.Services.Windows.GetXivWindowClientSize();

		this.Width = xivWindowSize.Width;
		this.Height = xivWindowSize.Height;

		this.Services.Windows.SetPosition(this, new(0, 0));
	}

	private void OnMouseEnterSelf(object sender, MouseEventArgs e)
	{
		this.Services.Windows.SendToBack(this);
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

	private void OnPreviewMouseMove(object sender, MouseEventArgs e)
	{
		this.Services.Windows.SendToBack(this);
	}
}
