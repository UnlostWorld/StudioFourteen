namespace StudioFourteen.Studio;

using StudioFourteen.Panels;
using StudioFourteen.Plugin;
using StudioFourteen.Settings;
using StudioFourteen.Utilities;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

public partial class BackgroundWindow : PanelWindow
{
	public static BackgroundWindow? Instance;

	public Persistence Persistence = new("Panel_BackgroundWindow");

	public BackgroundWindow()
	{
		Instance = this;
		this.ContentArea.DataContext = this;
	}

	public Point StudioButtonPosition
	{
		get => this.Persistence.GetPersistence<Point>();
		set => this.Persistence.SetPersistence(value);
	}

	protected override void OnOpened()
	{
		base.OnOpened();
		this.UpdatePosition();

		Thickness margin = this.StudioButton.Margin;
		margin.Left = this.StudioButtonPosition.X;
		margin.Top = this.StudioButtonPosition.Y;
		this.StudioButton.Margin = margin;
	}

	protected override void OnActivated(EventArgs e)
	{
		base.OnActivated(e);

		this.Services.Panels.ActivePanel = this;
		this.Services.Windows.SendToBack(this);
	}

	protected override void OnDeactivated(EventArgs e)
	{
		base.OnDeactivated(e);

		if (this.Services.Panels.ActivePanel == this)
		{
			this.Services.Panels.ActivePanel = null;
		}

		this.Services.Windows.SendToBack(this);
	}

	private void UpdatePosition()
	{
		if (this.Services.Windows.XivProcess == null)
			return;

		Rect xivWindowSize = this.Services.Windows.GetXivWindowSize();

		this.Width = xivWindowSize.Width - 16; // chrome margin
		this.Height = xivWindowSize.Height - this.Services.Windows.TitleBarHeight;

		this.Services.Windows.SetPosition(this, new(0, 0));
	}

	private void OnShutdownClicked(object sender, RoutedEventArgs e)
	{
		Task.Run(this.Services.Stop);
	}

	private void OnDragDelta(object sender, DragDeltaEventArgs e)
	{
		Thickness margin = this.StudioButton.Margin;
		margin.Left += e.HorizontalChange;
		margin.Top += e.VerticalChange;
		this.StudioButton.Margin = margin;

		Point pos = new(margin.Left, margin.Top);
		this.StudioButtonPosition = pos;
	}

	private void OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		this.Services.Input.HandleMouse(e, true);
	}

	private void OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		this.Services.Input.HandleMouse(e, false);
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		Point mousePos = e.GetPosition(this);
		this.Services.Input.HandleMouseMove(new((float)mousePos.X, (float)mousePos.Y));
	}

	private void OnMouseEnter(object sender, MouseEventArgs e)
	{
	}

	private void OnMouseLeave(object sender, MouseEventArgs e)
	{
		this.Services.Input.HandleMouseLeave();
	}

	private void OnMouseWheel(object sender, MouseWheelEventArgs e)
	{
		this.Services.Input.HandleMouseWheel(e.Delta / 120.0f);
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

		this.Services.Input.HandleKey(e.Key, true);
		e.Handled = true;
	}

	private void OnPreviewKeyUp(object sender, KeyEventArgs e)
	{
		if (e.IsRepeat)
			return;

		this.Services.Input.HandleKey(e.Key, false);
		e.Handled = true;
	}
}
