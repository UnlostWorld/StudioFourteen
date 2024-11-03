namespace StudioFourteen.Studio;

using ImGuiNET;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using StudioFourteen.Services;
using StudioFourteen.Settings;
using StudioFourteen.Utilities;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

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
		XivWindow.Activate();

		Thickness margin = this.StudioButton.Margin;
		margin.Left = this.StudioButtonPosition.X;
		margin.Top = this.StudioButtonPosition.Y;
		this.StudioButton.Margin = margin;
	}

	private void UpdatePosition()
	{
		if (XivWindow.Process == null)
			return;

		this.Width = XivWindow.Size.Width - 16; // chrome margin
		this.Height = XivWindow.Size.Height - XivWindow.TitleBarHeight;

		XivWindow.SetPosition(this, new(0, 0));
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
}
