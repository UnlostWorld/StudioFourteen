namespace StudioFourteen.Studio;

using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using StudioFourteen.Services;
using StudioFourteen.Settings;
using StudioFourteen.Utilities;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

public partial class BackgroundWindow : PanelWindow
{
	public static BackgroundWindow? Instance;

	public Persistence Persistence = new("BackgroundWindow");

	public BackgroundWindow()
	{
		Instance = this;
		this.ContentArea.DataContext = this;
	}

	[AutoNotify]
	public bool IsFullyLoaded
	{
		get
		{
			return this.Services.CurrentState == ServiceManagerBase.States.Started;
		}
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

		this.Width = XivWindow.Size.Width;
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
}
